namespace Spotlight
{
    using System.IO.MemoryMappedFiles;
    using System.Runtime.InteropServices;

    using Rage;

    [StructLayout(LayoutKind.Sequential, Pack = 1, Size = Size)]
    internal unsafe struct VehicleSpotlightStateData
    {
        public const int Size = 1 + 1 + 1 + 1 + 4 + 4 + (4 * 4) + (4 * 6);
        public enum Status : byte { Empty = 0, Released = 1, Used = 2 }

        public Status SlotStatus;
        public bool HasChanged;
        public bool IsActive;
        public bool IsInSearchMode;
        public uint VehicleHandle;
        public uint TrackedEntityHandle;
        public Quaternion Rotation;
        public Vector3 Position;

        public Entity TrackedEntity
        {
            get
            {
                return Rage.Native.NativeFunction.Natives.DoesEntityExist<bool>(TrackedEntityHandle) ?
                    World.GetEntityByHandle<Entity>(TrackedEntityHandle) :
                    null;
            }
            set
            {
                TrackedEntityHandle = value ? value.Handle : 0;
            }
        }

        public void Use(VehicleSpotlight spotlight)
        {
            SlotStatus = Status.Used;
            HasChanged = false;
            IsActive = spotlight.IsActive;
            IsInSearchMode = spotlight.IsInSearchMode;
            VehicleHandle = spotlight.Vehicle.Handle;
            TrackedEntity = spotlight.TrackedEntity;
            Rotation = spotlight.RelativeRotation;
            Position = spotlight.Position;
        }

        public void Release()
        {
            SlotStatus = Status.Released;
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    internal unsafe struct PluginStateData // ~16kb
    {
        public fixed byte SpotlightStates[VehicleSpotlightStateData.Size * MaxVehicleSpotlights];
        public fixed uint RequestVehicleHandles[MaxRequests];
        public uint RequestCount;
        [MarshalAs(UnmanagedType.I1)]
        public bool IsLoaded;

        // NOTE: this value is the next prime greater than 300, where 300 is the maximum number of vehicles possible
        public const int MaxVehicleSpotlights = 307;
        public const int MaxVehicleSpotlightsPrevPrime = 293; // needed for double hashing

        public const int MaxRequests = 8;
    }

    internal static unsafe class PluginState
    {
        private const string MappedFileName = "spotlight_plugin_state{D6561D3A-F1A2-4ABE-8BD3-7DBA2870F9BA}";

        private static MemoryMappedFile mappedFile;
        private static MemoryMappedViewAccessor mappedFileAccessor;
        private static PluginStateData* data;

        internal static void Init()
        {
            Game.LogTrivialDebug($"[Spotlight.PluginState] Init from '{System.AppDomain.CurrentDomain.FriendlyName}'");
            Game.LogTrivialDebug($"[Spotlight.PluginState] sizeof(PluginStateData) = '{sizeof(PluginStateData)}'");

            mappedFile = MemoryMappedFile.CreateOrOpen(MappedFileName, sizeof(PluginStateData));
            mappedFileAccessor = mappedFile.CreateViewAccessor(0, sizeof(PluginStateData));
            byte* ptr = null;
            mappedFileAccessor.SafeMemoryMappedViewHandle.AcquirePointer(ref ptr);

            data = (PluginStateData*)ptr;
        }

        internal static void Shutdown()
        {
            Game.LogTrivialDebug($"[Spotlight.PluginState] Shutdown from '{System.AppDomain.CurrentDomain.FriendlyName}'");

            if (mappedFileAccessor != null)
            {
                data = null;
                mappedFileAccessor.SafeMemoryMappedViewHandle.ReleasePointer();
                mappedFileAccessor.Dispose();
            }

            if (mappedFile != null)
            {
                mappedFile.Dispose();
            }
        }

        internal static bool IsLoaded
        {
            get => data != null && data->IsLoaded;
            set
            {
                if (data != null)
                {
                    data->IsLoaded = value;
                }
            }
        }

        private static uint DoubleHash(uint x, uint i)
        {
            const uint P = PluginStateData.MaxVehicleSpotlightsPrevPrime;

            uint h2 = P - (x % P);
            return (x + i * h2) % PluginStateData.MaxVehicleSpotlights;
        }

        private const uint MaxFindAttempts = 1000;
        private const uint InvalidIndex = 0xFFFFFFFF;
        private static uint FindSpotlightIndex(Vehicle vehicle)
        {
            uint handle = vehicle.Handle;
            uint index = 0;
            uint i = 0;
            do
            {
                index = DoubleHash(handle, i++);

                VehicleSpotlightStateData* data = At(index);
                if (data->SlotStatus == VehicleSpotlightStateData.Status.Used && data->VehicleHandle == handle)
                {
                    return index;
                }
                else if (data->SlotStatus == VehicleSpotlightStateData.Status.Empty)
                {
                    break;
                }
            } while (i <= MaxFindAttempts);

            return InvalidIndex;
        }

        private static uint FindSpotlightInsertionIndex(Vehicle vehicle)
        {
            uint handle = vehicle.Handle;
            uint index = 0;
            uint i = 0;
            do
            {
                index = DoubleHash(handle, i++);
                if (At(index)->SlotStatus != VehicleSpotlightStateData.Status.Used)
                {
                    return index;
                }
            } while (i <= MaxFindAttempts);

            return InvalidIndex;
        }

        private static VehicleSpotlightStateData* At(uint index)
        {
            byte* buffer = &data->SpotlightStates[VehicleSpotlightStateData.Size * index];
            return (VehicleSpotlightStateData*)buffer;
        }

        private static VehicleSpotlightStateData* GetSpotlightState(Vehicle vehicle)
        {
            uint index = FindSpotlightIndex(vehicle);
            if (index != InvalidIndex)
            {
                return At(index);
            }
            else
            {
                return null;
            }
        }

        internal static bool HasSpotlight(Vehicle vehicle)
        {
            return FindSpotlightIndex(vehicle) != InvalidIndex;
        }

        internal static VehicleSpotlightStateData* AddSpotlight(VehicleSpotlight spotlight)
        {
            uint index = FindSpotlightInsertionIndex(spotlight.Vehicle);
            if (index != InvalidIndex)
            {
                VehicleSpotlightStateData* s = At(index);
                s->Use(spotlight);
                return s;
            }
            else
            {
                throw new System.InvalidOperationException("index is invalid");
            }
        }

        internal static void SetSpotlightTrackedEntity(Vehicle vehicle, Entity entity)
        {
            VehicleSpotlightStateData* s = GetSpotlightState(vehicle);
            if (s != null)
            {
                s->TrackedEntity = entity;
                s->HasChanged = true;
            }
        }

        internal static Entity GetSpotlightTrackedEntity(Vehicle vehicle)
        {
            VehicleSpotlightStateData* s = GetSpotlightState(vehicle);

            return s != null ? s->TrackedEntity : null;
        }

        internal static void SetSpotlightActive(Vehicle vehicle, bool active)
        {
            VehicleSpotlightStateData* s = GetSpotlightState(vehicle);
            if (s != null)
            {
                s->IsActive = active;
                s->HasChanged = true;
            }
        }

        internal static bool IsSpotlightActive(Vehicle vehicle)
        {
            VehicleSpotlightStateData* s = GetSpotlightState(vehicle);

            return s != null ? s->IsActive : false;
        }

        internal static void SetSpotlightInSearchMode(Vehicle vehicle, bool searchModeActive)
        {
            VehicleSpotlightStateData* s = GetSpotlightState(vehicle);
            if (s != null)
            {
                s->IsInSearchMode = searchModeActive;
                s->HasChanged = true;
            }
        }

        internal static bool IsSpotlightInSearchMode(Vehicle vehicle)
        {
            VehicleSpotlightStateData* s = GetSpotlightState(vehicle);

            return s != null ? s->IsInSearchMode : false;
        }

        internal static void SetSpotlightRotation(Vehicle vehicle, Quaternion rotation)
        {
            VehicleSpotlightStateData* s = GetSpotlightState(vehicle);
            if (s != null)
            {
                s->Rotation = rotation;
                s->HasChanged = true;
            }
        }

        internal static Quaternion GetSpotlightRotation(Vehicle vehicle)
        {
            VehicleSpotlightStateData* s = GetSpotlightState(vehicle);

            return s != null ? s->Rotation : default;
        }

        internal static Vector3 GetSpotlightPosition(Vehicle vehicle)
        {
            VehicleSpotlightStateData* s = GetSpotlightState(vehicle);

            return s != null ? s->Position : default;
        }

        internal static bool RequestSpotlight(Vehicle vehicle)
        {
            if (!IsLoaded || HasSpotlight(vehicle))
            {
                return false;
            }

            GameFiber.WaitUntil(() => data->RequestCount < PluginStateData.MaxRequests);

            data->RequestVehicleHandles[data->RequestCount++] = vehicle.Handle;
            return true;
        }

        internal static void RequestSpotlightAndWait(Vehicle vehicle)
        {
            if (!RequestSpotlight(vehicle))
            {
                return;
            }

            GameFiber.WaitUntil(() => HasSpotlight(vehicle));
        }

        internal static bool HasAnySpotlightRequest()
        {
            return data->RequestCount > 0;
        }

        internal static Vehicle PopSpotlightRequest()
        {
            uint handle = data->RequestVehicleHandles[--data->RequestCount];
            if (Rage.Native.NativeFunction.Natives.DoesEntityExist<bool>(handle))
            {
                return World.GetEntityByHandle<Vehicle>(handle);
            }
            else
            {
                return null;
            }
        }
    }
}
