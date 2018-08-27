namespace Spotlight
{
    using System;
    using System.Linq;
    using System.Collections.Generic;
    
    using Rage;
    using Rage.Native;

    using Spotlight.Core;
    using Spotlight.Core.Memory;
    using Spotlight.InputControllers;

    // TODO: can probably optimize some of the rotations calculations that happen in each update
    internal unsafe class VehicleSpotlight : BaseSpotlight
    {
        private const eBoneRefId InvalidBoneRefId = (eBoneRefId)(-1);
        private const uint VehicleWeaponSearchlightHash = 0xCDAC517D; // VEHICLE_WEAPON_SEARCHLIGHT
        private const float SearchModeSpeed = 0.365f;
        private static readonly Quaternion HeliSearchModeStart = Quaternion.FromRotation(new Rotator(-32.5f, 0.0f, -40.0f));
        private static readonly Quaternion HeliSearchModeEnd = Quaternion.FromRotation(new Rotator(-32.5f, 0.0f, 40.0f));
        private static readonly Quaternion DefaultSearchModeStart = Quaternion.FromRotation(new Rotator(-3.25f, 0.0f, -40.0f));
        private static readonly Quaternion DefaultSearchModeEnd = Quaternion.FromRotation(new Rotator(-3.25f, 0.0f, 40.0f));

        private readonly CVehicle* nativeVehicle;
        private Entity trackedEntity;

        public Vehicle Vehicle { get; }
        public VehicleData VehicleData { get; }
        
        public Rotator RelativeRotation { get; set; }

        public bool IsTrackingEntity { get { return TrackedEntity.Exists(); } }
        public Entity TrackedEntity
        {
            get => trackedEntity;
            set
            {
                if(value != trackedEntity)
                {
                    trackedEntity = value;
                    OnTrackedEntityChanged();
                }
            }
        }
        
        public bool IsInSearchMode { get; set; }
        private float searchModePercentage;
        private bool searchModeDir;

        public bool IsCurrentPlayerVehicleSpotlight { get { return Vehicle == Game.LocalPlayer.Character.CurrentVehicle; } }

        public override bool IsActive
        {
            get => base.IsActive;
            set
            {
                if (enableTurret && !value)
                {
                    RestoreNativeTurret();
                }

                justActivated = value;
                base.IsActive = value;
            }
        }

        private bool justActivated;

        public override Vector3 Direction
        {
            get => base.Direction;
            set
            {
                if (value != Direction)
                {
                    SetDirectionInternal(value);
                    base.Direction = value;
                }
            }
        }

        // turret stuff
        public VehicleBone WeaponBone { get; private set; }
        public VehicleBone TurretBaseBone { get; private set; }
        // barrel is optional
        public VehicleBone TurretBarrelBone { get; private set; }

        private bool enableTurret = false;

        private int nativeWeaponIndex = -1;
        private int nativeTurretIndex = -1;
        private eBoneRefId nativeTurretBaseBoneRefId = InvalidBoneRefId, nativeTurretBarrelBoneRefId = InvalidBoneRefId;
        private ushort[] weaponBoneHierarchy;
        private int extraLightEmissiveIndex = -1;

        // TODO: small lag when executing for the first time, probably due to JIT compilation, figure out if there's a fix
        public VehicleSpotlight(Vehicle vehicle) : base(GetSpotlightDataForModel(vehicle.Model))
        {
            Vehicle = vehicle;
            nativeVehicle = (CVehicle*)vehicle.MemoryAddress;
            VehicleData = GetVehicleDataForModel(vehicle.Model);
            if (!VehicleData.DisableTurret)
            {
                TryFindTurretStuff();
            }

            if (vehicle.Model.IsHelicopter)
                RelativeRotation = new Rotator(-50.0f, 0.0f, 0.0f);
        }

        // attempts to gather the necessary data to enable the turret movement 
        private void TryFindTurretStuff()
        {
            RestoreNativeTurret();

            enableTurret = false;
            nativeWeaponIndex = -1;
            nativeTurretIndex = -1;
            nativeTurretBaseBoneRefId = InvalidBoneRefId;
            nativeTurretBarrelBoneRefId = InvalidBoneRefId;

            CVehicleWeaponMgr* weaponMgr = nativeVehicle->GetWeaponMgr();
            if (weaponMgr != null)
            {
                for (int i = 0; i < weaponMgr->WeaponCount; i++)
                {
                    CVehicleWeapon* weapon = weaponMgr->GetWeapon(i);
                    if (weapon != null)
                    {
                        if (weapon->GetName() == VehicleWeaponSearchlightHash)
                        {
                            nativeWeaponIndex = i;
                            break;
                        }
                    }
                    else
                    {
                        break;
                    }
                }

                Game.LogTrivialDebug($"[Weapon Search] Index -> {nativeWeaponIndex}");

                if (nativeWeaponIndex != -1)
                {
                    eBoneRefId GetTurretBoneRefIdForIndex(ushort boneIndex)
                    {
                        eBoneRefId[] ids = new[]
                        {
                                eBoneRefId.turret_1base, eBoneRefId.turret_2base, eBoneRefId.turret_3base, eBoneRefId.turret_4base,
                                eBoneRefId.turret_1barrel, eBoneRefId.turret_2barrel, eBoneRefId.turret_3barrel, eBoneRefId.turret_4barrel,
                            };

                        foreach (eBoneRefId i in ids)
                        {
                            if (nativeVehicle->GetBoneIndex(i) == boneIndex)
                            {
                                return i;
                            }
                        }
                        return InvalidBoneRefId;
                    }

                    bool IsTurretBaseBone(ushort boneIndex)
                    {
                        eBoneRefId id = GetTurretBoneRefIdForIndex(boneIndex);
                        return Array.Exists(new[] { eBoneRefId.turret_1base, eBoneRefId.turret_2base, eBoneRefId.turret_3base, eBoneRefId.turret_4base }, (e) => e == id);
                    }

                    bool IsTurretBarrelBone(ushort boneIndex)
                    {
                        eBoneRefId id = GetTurretBoneRefIdForIndex(boneIndex);
                        return Array.Exists(new[] { eBoneRefId.turret_1barrel, eBoneRefId.turret_2barrel, eBoneRefId.turret_3barrel, eBoneRefId.turret_4barrel }, (e) => e == id);
                    }

                    eBoneRefId nativeWeaponBoneRefId = weaponMgr->GetWeapon(nativeWeaponIndex)->weaponBoneRefId;
                    crSkeletonData* skel = nativeVehicle->inst->archetype->skeleton->skeletonData;
                    crSkeletonBoneData* weaponBoneData = &skel->bones[nativeVehicle->GetBoneIndex(nativeWeaponBoneRefId)];

                    crSkeletonBoneData* currentBoneData = weaponBoneData;
                    eBoneRefId turretBaseBoneRefId = InvalidBoneRefId;
                    eBoneRefId turretBarrelBoneRefId = InvalidBoneRefId;
                    do
                    {
                        if (currentBoneData->parentIndex == 0)
                        {
                            break;
                        }
                        else
                        {
                            ushort parentIndex = currentBoneData->parentIndex;

                            if (turretBaseBoneRefId == InvalidBoneRefId)
                            {
                                if (IsTurretBaseBone(parentIndex))
                                {
                                    turretBaseBoneRefId = GetTurretBoneRefIdForIndex(parentIndex);
                                    break;
                                }
                                else if (turretBarrelBoneRefId == InvalidBoneRefId && IsTurretBarrelBone(parentIndex))
                                {
                                    turretBarrelBoneRefId = GetTurretBoneRefIdForIndex(parentIndex);
                                }
                            }

                            currentBoneData = &skel->bones[parentIndex];
                        }
                    }
                    while (turretBaseBoneRefId == InvalidBoneRefId);

                    Game.LogTrivialDebug($"[Bone Search] Weapon -> {(int)nativeWeaponBoneRefId}, Turret Base -> {turretBaseBoneRefId.ToString()} ({(int)turretBaseBoneRefId}), Turret Barrel -> {turretBarrelBoneRefId.ToString()} ({(int)turretBarrelBoneRefId})");

                    if (turretBaseBoneRefId != InvalidBoneRefId)
                    {
                        nativeTurretIndex = -1;
                        for (int i = 0; i < weaponMgr->TurretCount; i++)
                        {
                            CTurret* t = weaponMgr->GetTurret(i);
                            if (t != null)
                            {
                                if (t->baseBoneRefId == turretBaseBoneRefId)
                                {
                                    nativeTurretIndex = i;
                                    break;
                                }
                            }
                        }

                        Game.LogTrivialDebug($"[Turret Search] Index -> {nativeTurretIndex}");

                        if (nativeTurretIndex != -1)
                        {
                            nativeTurretBaseBoneRefId = turretBaseBoneRefId;
                            nativeTurretBarrelBoneRefId = turretBarrelBoneRefId;

                            if (nativeWeaponBoneRefId != InvalidBoneRefId)
                            {
                                byte i = nativeVehicle->GetBoneIndex(nativeWeaponBoneRefId);
                                if (i != 0xFF)
                                {
                                    int nativeWeaponBoneIndex = i;
                                    if (!VehicleBone.TryGetForVehicle(Vehicle, nativeWeaponBoneIndex, out VehicleBone weaponBone))
                                    {
                                        throw new InvalidOperationException($"The model \"{Vehicle.Model.Name}\" doesn't have the bone of index {nativeWeaponBoneIndex} for the CVehicleWeapon Bone");
                                    }
                                    WeaponBone = weaponBone;
                                }
                            }

                            if (nativeTurretBaseBoneRefId != InvalidBoneRefId)
                            {
                                byte i = nativeVehicle->GetBoneIndex(nativeTurretBaseBoneRefId);
                                if (i != 0xFF)
                                {
                                    int nativeTurretBaseBoneIndex = i;
                                    if (!VehicleBone.TryGetForVehicle(Vehicle, nativeTurretBaseBoneIndex, out VehicleBone baseBone))
                                    {
                                        throw new InvalidOperationException($"The model \"{Vehicle.Model.Name}\" doesn't have the bone of index {nativeTurretBaseBoneIndex} for the CTurret Base Bone");
                                    }
                                    TurretBaseBone = baseBone;
                                }
                            }

                            if (nativeTurretBarrelBoneRefId != InvalidBoneRefId)
                            {
                                byte i = nativeVehicle->GetBoneIndex(nativeTurretBarrelBoneRefId);
                                if (i != 0xFF)
                                {
                                    int nativeTurretBarrelBoneIndex = i;
                                    VehicleBone.TryGetForVehicle(Vehicle, nativeTurretBarrelBoneIndex, out VehicleBone barrelBone);
                                    TurretBarrelBone = barrelBone;
                                }
                            }


                            List<ushort> weaponBoneHierarchyList = new List<ushort>();
                            ushort idx = (ushort)WeaponBone.Index;
                            do
                            {
                                weaponBoneHierarchyList.Add(idx);
                                ushort parent = nativeVehicle->inst->archetype->skeleton->skeletonData->bones[idx].parentIndex;
                                idx = parent;
                            } while (idx != 0xFFFF);
                            weaponBoneHierarchy = weaponBoneHierarchyList.ToArray();
                            Game.LogTrivialDebug("[Weapon Bone Hierarchy] " + String.Join(" -> ", weaponBoneHierarchyList));

                            enableTurret = true;

                            if (!Plugin.Settings.Vehicles.Data.ContainsValue(VehicleData))
                            {
                                VehicleData.Offset = new XYZ(0.0f, 0.0f, 0.0f);
                            }

                            if (Plugin.Settings.EnableLightEmissives && VehicleData.SpotlightExtraLight == VehicleData.DefaultSpotlightExtraLight)
                            {
                                byte[] extraLightIndices = new byte[4];
                                extraLightIndices[0] = nativeVehicle->GetBoneIndex(eBoneRefId.extralight_1);
                                extraLightIndices[1] = nativeVehicle->GetBoneIndex(eBoneRefId.extralight_2);
                                extraLightIndices[2] = nativeVehicle->GetBoneIndex(eBoneRefId.extralight_3);
                                extraLightIndices[3] = nativeVehicle->GetBoneIndex(eBoneRefId.extralight_4);

                                for (int i = 0; i < 4; i++)
                                {
                                    byte boneIndex = extraLightIndices[i];
                                    if(boneIndex != 0xFF)
                                    { 
                                        // check if the light bone is child of any of the bones in the turret hierarchy
                                        ushort parent = nativeVehicle->inst->archetype->skeleton->skeletonData->bones[boneIndex].parentIndex;
                                        if (Array.FindIndex(weaponBoneHierarchy, (ushort weaponBone) => weaponBone == parent) != -1)
                                        {
                                            eBoneRefId id = eBoneRefId.extralight_1 + (i);
                                            extraLightEmissiveIndex = GameFunctions.GetLightEmissiveIndexForBone(id);
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            if (VehicleData.SpotlightExtraLight != VehicleData.DefaultSpotlightExtraLight)
            {
                eBoneRefId id = eBoneRefId.extralight_1 + (VehicleData.SpotlightExtraLight - 1);
                if (nativeVehicle->GetBoneIndex(id) != 0xFF)
                {
                    extraLightEmissiveIndex = GameFunctions.GetLightEmissiveIndexForBone(id);
                }
            }
        }

        internal void OnDisableTurretChanged()
        {
            bool disableTurret = VehicleData.DisableTurret;
            if (disableTurret)
            {
                RestoreNativeTurret();
                enableTurret = false;
            }
            else
            {
                TryFindTurretStuff();
            }
        }

        private void RestoreNativeTurret()
        {
            if (enableTurret && Vehicle)
            {
                CVehicleWeaponMgr* weaponMgr = nativeVehicle->GetWeaponMgr();
                if (weaponMgr != null)
                {
                    CTurret* turret = weaponMgr->GetTurret(nativeTurretIndex);
                    turret->baseBoneRefId = nativeTurretBaseBoneRefId;
                    turret->barrelBoneRefId = nativeTurretBarrelBoneRefId;
                }
            }
        }

        private Quaternion GetSearchModeRotation()
        {
            bool isHeli = Vehicle.IsHelicopter;
            Quaternion q = Quaternion.Slerp(isHeli ? HeliSearchModeStart : DefaultSearchModeStart,
                                            isHeli ? HeliSearchModeEnd : DefaultSearchModeEnd, 
                                            searchModePercentage);

            searchModePercentage += SearchModeSpeed * Game.FrameTime * (searchModeDir ? 1.0f : -1.0f);
            if((searchModeDir && searchModePercentage >= 1.0f) ||
                (!searchModeDir && searchModePercentage <= 0.0f))
            {
                searchModeDir = !searchModeDir;
            }

            return q;
        }

        public void OnUnload()
        {
            RestoreNativeTurret();
        }
        
        public void Update(IList<SpotlightInputController> controllers)
        {
            if (!IsActive)
                return;
            
            if (enableTurret)
            {
                // invalidate turret bones so the game code doesn't reset their rotation
                CVehicleWeaponMgr* weaponMgr = nativeVehicle->GetWeaponMgr();
                if (weaponMgr != null)
                {
                    CTurret* turret = weaponMgr->GetTurret(nativeTurretIndex);
                    turret->baseBoneRefId = InvalidBoneRefId;
                    turret->barrelBoneRefId = InvalidBoneRefId;

                    if (justActivated)
                    {
                        // if just activated, take the current turret rotation as the spotlight's rotation 
                        // so the bone doesn't rotate abruptly
                        RelativeRotation = turret->rot1.ToRotation();
                    }
                }
            }

            if (IsCurrentPlayerVehicleSpotlight)
            {
                for (int i = 0; i < controllers.Count; i++)
                {
                    controllers[i].UpdateControls(this);
                    if (!IsTrackingEntity && !IsInSearchMode && controllers[i].GetUpdatedRotationDelta(this, out Rotator newRotDelta))
                    {
                        RelativeRotation += newRotDelta;
                        break;
                    }
                }
            }

            if (enableTurret)
            {
                // GetBoneOrientation sometimes seems to return an non-normalized quaternion, such as [X:-8 Y:8 Z:-8 W:0], or 
                // a quaternion with NaN values, which fucks up the bone transform and makes it disappear
                // not sure if it's an issue with my code for changing the bone rotation or if the issue is in GetBoneOrientation
                Quaternion boneRot = Vehicle.GetBoneOrientation(WeaponBone.Index);
                boneRot.Normalize();
                if (Single.IsNaN(boneRot.X) || Single.IsNaN(boneRot.Y) || Single.IsNaN(boneRot.Z) || Single.IsNaN(boneRot.W))
                {
                    boneRot = Quaternion.Identity;
                }

                Position = MathHelper.GetOffsetPosition(Vehicle.GetBonePosition(WeaponBone.Index), boneRot, VehicleData.Offset);

                Vector3 dir = Vector3.Zero;
                if (IsTrackingEntity)
                {
                    dir = (TrackedEntity.Position - Position).ToNormalized();
                }
                else if (IsInSearchMode)
                {
                    Quaternion q = GetSearchModeRotation() * Vehicle.Orientation;
                    q.Normalize();
                    dir = q.ToVector();
                }
                else
                {
                    Quaternion q = RelativeRotation.ToQuaternion() * Vehicle.Orientation;
                    q.Normalize();
                    dir = q.ToVector();
                }

                Direction = dir;
            }
            else
            {
                Position = Vehicle.GetOffsetPosition(VehicleData.Offset);
                Direction = IsTrackingEntity ? (TrackedEntity.Position - Position).ToNormalized() :
                            IsInSearchMode ? (GetSearchModeRotation() * Vehicle.Orientation).ToVector() :
                            (Vehicle.Rotation + RelativeRotation).ToVector();
            }

            justActivated = false;

            DrawLight();
        }

        private void SetDirectionInternal(Vector3 worldDirection)
        {
            if (enableTurret)
            {
                Quaternion GetLocalRotation(int boneIndex, Quaternion worldRotation)
                {
                    Quaternion CalculateBoneWorldRotationMatrix(int parentBoneIndex)
                    {
                        List<ushort> hierarchy = new List<ushort>();
                        ushort idx = (ushort)parentBoneIndex;
                        do
                        {
                            hierarchy.Add(idx);
                            ushort parent = nativeVehicle->inst->archetype->skeleton->skeletonData->bones[idx].parentIndex;
                            idx = parent;
                        } while (idx != 0xFFFF);


                        Matrix rotationMatrix = *nativeVehicle->inst->archetype->skeleton->entityTransform;
                        for (int i = hierarchy.Count - 1; i >= 0; i--)
                        {
                            Matrix left = nativeVehicle->inst->archetype->skeleton->desiredBonesTransformsArray[hierarchy[i]];
                            left.M14 = 0.0f;
                            left.M24 = 0.0f;
                            left.M34 = 0.0f;
                            left.M44 = 1.0f;

                            Matrix right = rotationMatrix;
                            right.M14 = 0.0f;
                            right.M24 = 0.0f;
                            right.M34 = 0.0f;
                            right.M44 = 1.0f;

                            rotationMatrix = Matrix.Multiply(left, right);
                        }

                        Utility.DecomposeMatrix(rotationMatrix, out _, out Quaternion r, out _);
                        return r;
                    }

                    Quaternion rot = CalculateBoneWorldRotationMatrix(nativeVehicle->inst->archetype->skeleton->skeletonData->bones[boneIndex].parentIndex);
                    rot = (worldRotation * Quaternion.Invert(rot));
                    return rot;
                }

                Quaternion world = worldDirection.ToQuaternion();
                Quaternion local = GetLocalRotation(TurretBaseBone.Index, world);

                CVehicleWeaponMgr* weaponMgr = nativeVehicle->GetWeaponMgr();
                if (weaponMgr != null)
                {
                    // changes turret rotation so when deactivating the spotlight
                    // the game code doesn't reset the turret bone rotation
                    CTurret* turret = weaponMgr->GetTurret(nativeTurretIndex);
                    turret->rot1 = local;
                    turret->rot2 = local;
                    turret->rot3 = local;
                }

                if (TurretBarrelBone == null)
                {
                    TurretBaseBone.SetRotation(local);
                }
                else
                {
                    Vector3 baseWorldDir = worldDirection;
                    Vector3 baseLocalDir = GetLocalRotation(TurretBaseBone.Index, baseWorldDir.ToQuaternion()).ToVector();
                    baseLocalDir.Z = 0.0f;
                    Quaternion baseLocal = TurretBaseBone.OriginalRotation * baseLocalDir.ToQuaternion();
                    TurretBaseBone.SetRotation(baseLocal);

                    Vector3 barrelWorldDir = worldDirection;
                    Quaternion barrelLocal = GetLocalRotation(TurretBarrelBone.Index, barrelWorldDir.ToQuaternion());
                    TurretBarrelBone.SetRotation(barrelLocal);
                }
            }
        }

        public void SetExtraLightEmissive()
        {
            if(IsActive && extraLightEmissiveIndex != -1)
            {
                nativeVehicle->SetLightEmissive(extraLightEmissiveIndex, 10.0f); // TODO: get light emissive value from settings or based on the spotlight data
            }
        }

        static string TrackerVersionNumber;
        private void OnTrackedEntityChanged()
        {
            const string NotificationTitle = "~h~Spotlight Tracker";
            if (TrackerVersionNumber == null)
            {
                // generate random version number that increases over time
                DateTime now = DateTime.UtcNow;
                uint major = (uint)(now.Year - 2017);
                uint minor = (uint)(now.Month - 1);
                uint revision = (uint)(new DateTime(now.Year, now.Month, now.Day > 16 ? 16 : 1).DayOfYear * now.Month * 1.345f);
                TrackerVersionNumber = $"~b~<font size='8'>VERSION {major}.{minor}.{revision}</font>";
            }


            Entity e = TrackedEntity;
            if (e)
            {
                if (e is Vehicle veh)
                {
                    string text = "Tracking vehicle.";
                    CVehicle* vehPtr = (CVehicle*)veh.MemoryAddress;
                    IntPtr makeNamePtr = vehPtr->GetMakeName();
                    IntPtr gameNamePtr = vehPtr->GetGameName();
                    string makeName = Utility.IsStringEmpty(makeNamePtr) ? null : Utility.GetLocalizedString(makeNamePtr);
                    string gameName = Utility.IsStringEmpty(gameNamePtr) ? null : Utility.GetLocalizedString(gameNamePtr);
                    text += "~n~Model: " + makeName + " " + gameName;
                    if(veh.LicensePlateType != LicensePlateType.None)
                    {
                    text += "~n~License Plate: " + veh.LicensePlate;
                    }
                    text += "~n~Distance: " + ((int)Vehicle.DistanceTo(e)) + "m";


                    string txd = "mpcarhud";
                    string txn = "transport_car_icon";
                    if (veh.IsBicycle)
                    {
                        txn = "transport_bicycle_icon";
                    }
                    else if (veh.IsBike)
                    {
                        txn = "transport_bike_icon";
                    }
                    else if (veh.IsBoat)
                    {
                        txn = "transport_boat_icon";
                    }
                    else if (veh.IsHelicopter)
                    {
                        txn = "transport_heli_icon";
                    }
                    else if (veh.IsPlane)
                    {
                        txn = "transport_plane_icon";
                    }

                    Game.DisplayNotification(txd, txn, NotificationTitle, TrackerVersionNumber, text);
                }
                else if (e is Ped ped)
                {
                    GameFiber.StartNew(() =>
                    {
                        uint headshotHandle = NativeFunction.Natives.RegisterPedheadshot<uint>(ped);
                        int startTime = Environment.TickCount;
                        while ((Environment.TickCount - startTime) < 10000) // max wait is 10 seconds
                        {
                            if (NativeFunction.Natives.IsPedheadshotReady<bool>(headshotHandle))
                            {
                                string text = "Tracking suspect.";
                                text += "~n~Distance: " + ((int)Vehicle.DistanceTo(e)) + "m";

                                string txd = NativeFunction.Natives.GetPedheadshotTxdString<string>(headshotHandle);
                                Game.DisplayNotification(txd, txd, NotificationTitle, TrackerVersionNumber, text);
                                break;
                            }
                            GameFiber.Sleep(5);
                        }
                        NativeFunction.Natives.UnregisterPedheadshot<uint>(headshotHandle);
                    });
                }
            }
            else
            {
                Game.DisplayNotification("timerbar_sr", "timer_cross", NotificationTitle, TrackerVersionNumber, "Stopped tracking.");
            }
        }

        internal static VehicleData GetVehicleDataForModel(Model model)
        {
            foreach (KeyValuePair<string, VehicleData> entry in Plugin.Settings.Vehicles.Data)
            {
                if (model == new Model(entry.Key))
                    return entry.Value;
            }
            
            return VehicleData.Default;
        }

        internal static SpotlightData GetSpotlightDataForModel(Model model)
        {
            return model.IsBoat ? Plugin.Settings.Visual.Boat :
                   model.IsHelicopter ? Plugin.Settings.Visual.Helicopter : Plugin.Settings.Visual.Default;
        }
    }
}
