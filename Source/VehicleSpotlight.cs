﻿namespace Spotlight
{
    using System;
    using System.Linq;
    using System.Collections.Generic;

    using Rage;
    using Rage.Native;

    using Spotlight.Core;
    using Spotlight.Core.Memory;
    using Spotlight.InputControllers;

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
        private readonly VehicleSpotlightStateData* state;
        private Entity trackedEntity;
        private Quaternion relativeRotation;
        private bool isInSearchMode;

        public Vehicle Vehicle { get; }
        public VehicleData VehicleData { get; }

        public Quaternion RelativeRotation
        {
            get => relativeRotation;
            set
            {
                if (value != relativeRotation)
                {
                    state->Rotation = value;
                    relativeRotation = value;
                }
            }
        }

        public bool IsTrackingEntity { get { return TrackedEntity.Exists(); } }
        public Entity TrackedEntity
        {
            get => trackedEntity;
            set
            {
                if (value != trackedEntity)
                {
                    state->TrackedEntity = value;
                    trackedEntity = value;
                }
            }
        }

        public bool IsInSearchMode
        {
            get => isInSearchMode;
            set
            {
                if (value != isInSearchMode)
                {
                    state->IsInSearchMode = value;
                    isInSearchMode = value;
                }
            }
        }

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

                state->IsActive = value;
                justActivated = value;
                base.IsActive = value;
            }
        }

        private bool justActivated;

        public override Vector3 Position
        {
            get => base.Position;
            set
            {
                if (value != Position)
                {
                    state->Position = value;
                    base.Position = value;
                }
            }
        }

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
        private int extraLightEmissiveIndex = -1;

        // TODO: small lag when executing for the first time, probably due to JIT compilation, figure out if there's a fix
        public VehicleSpotlight(Vehicle vehicle) : base(GetSpotlightDataForModel(vehicle.Model))
        {
            Vehicle = vehicle;
            nativeVehicle = (CVehicle*)vehicle.MemoryAddress;
            VehicleData = GetVehicleDataForModel(vehicle.Model);
            state = PluginState.AddSpotlight(this);

            if (!VehicleData.DisableTurret)
            {
                TryFindTurretStuff();
            }

            if (vehicle.Model.IsHelicopter)
            {
                RelativeRotation = Quaternion.FromRotation(new Rotator(-50.0f, 0.0f, 0.0f));
            }
            else
            {
                RelativeRotation = Quaternion.Identity;
            }
        }

        public void OnRemoved()
        {
            state->Release();
        }

        private void SyncWithState()
        {
            if (!state->HasChanged)
            {
                return;
            }

            IsActive = state->IsActive;
            IsInSearchMode = state->IsInSearchMode;
            TrackedEntity = state->TrackedEntity;
            RelativeRotation = state->Rotation;

            state->HasChanged = false;
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
                    crSkeletonData* skel = nativeVehicle->inst->entry->skeleton->skeletonData;
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
                                ushort parent = nativeVehicle->inst->entry->skeleton->skeletonData->bones[idx].parentIndex;
                                idx = parent;
                            } while (idx != 0 && idx != 0xFFFF); // until idx is root or doesn't exists
                            Game.LogTrivialDebug("[Weapon Bone Hierarchy] " + String.Join(" -> ", weaponBoneHierarchyList));

                            enableTurret = true;

                            if (!Plugin.Settings.Vehicles.Data.ContainsValue(VehicleData))
                            {
                                VehicleData.Offset = new XYZ(0.0f, 0.0f, 0.0f);
                            }

                            if (Plugin.Settings.EnableLightEmissives && VehicleData.SpotlightExtraLight == VehicleData.DefaultSpotlightExtraLight)
                            {
                                byte[] extraLightIndices = new[]
                                {
                                    nativeVehicle->GetBoneIndex(eBoneRefId.extralight_1),
                                    nativeVehicle->GetBoneIndex(eBoneRefId.extralight_2),
                                    nativeVehicle->GetBoneIndex(eBoneRefId.extralight_3),
                                    nativeVehicle->GetBoneIndex(eBoneRefId.extralight_4),
                                };

                                for (int i = 0; i < extraLightIndices.Length; i++)
                                {
                                    byte boneIndex = extraLightIndices[i];
                                    Game.LogTrivialDebug($"[ExtraLight Search] #{i} (boneIndex:{boneIndex})");
                                    if (boneIndex != 0xFF)
                                    {
                                        // check if the light bone is child of any of the bones in the turret hierarchy
                                        ushort parent = nativeVehicle->inst->entry->skeleton->skeletonData->bones[boneIndex].parentIndex;
                                        Game.LogTrivialDebug($"[ExtraLight Search]  > Parent = {parent}");
                                        if (weaponBoneHierarchyList.FindIndex(weaponBone => weaponBone == parent) != -1)
                                        {
                                            Game.LogTrivialDebug("[ExtraLight Search]    in hierarchy");
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

            if (!Game.IsPaused)
            {
                searchModePercentage += SearchModeSpeed * Game.FrameTime * (searchModeDir ? 1.0f : -1.0f);
            }

            if ((searchModeDir && searchModePercentage >= 1.0f) ||
                (!searchModeDir && searchModePercentage <= 0.0f))
            {
                searchModeDir = !searchModeDir;
            }

            return q;
        }

        public void OnUnload()
        {
            OnRemoved();
            RestoreNativeTurret();
        }

        public void Update(IList<SpotlightInputController> controllers)
        {
            SyncWithState();

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
                        RelativeRotation = turret->rot1;
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
                        Rotator oldRot = RelativeRotation.ToRotation();
                        Rotator newRot = oldRot + newRotDelta;
                        newRot.Roll = 0.0f;
                        RelativeRotation = newRot.ToQuaternion();
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
                    Quaternion q = RelativeRotation * Vehicle.Orientation;
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
                            (RelativeRotation * Vehicle.Orientation).ToVector();
            }

            if (IsTrackingEntity || IsInSearchMode)
            {
                // keep updating the RelativeRotation when tracking or search mode so it's available through the API
                RelativeRotation = Direction.ToQuaternion() * Quaternion.Invert(Vehicle.Orientation);
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
                        crSkeleton* skel = nativeVehicle->inst->entry->skeleton;
                        crSkeletonData* skelData = skel->skeletonData;
                        ushort* hierarchy = stackalloc ushort[skel->bonesCount];
                        int hierarchyCount = 0;
                        ushort idx = (ushort)parentBoneIndex;
                        do
                        {
                            hierarchy[hierarchyCount++] = idx;
                            idx = skelData->bones[idx].parentIndex;
                        } while (idx != 0xFFFF);


                        Matrix rotationMatrix = *skel->entityTransform;
                        for (int i = hierarchyCount - 1; i >= 0; i--)
                        {
                            Matrix left = skel->desiredBonesTransformsArray[hierarchy[i]];
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

                    Quaternion rot = CalculateBoneWorldRotationMatrix(nativeVehicle->inst->entry->skeleton->skeletonData->bones[boneIndex].parentIndex);
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
                    Vector3 barrelLocalDir = GetLocalRotation(TurretBarrelBone.Index, barrelWorldDir.ToQuaternion()).ToVector();
                    barrelLocalDir.X = 0.0f;
                    Quaternion barrelLocal = TurretBarrelBone.OriginalRotation * barrelLocalDir.ToQuaternion();
                    TurretBarrelBone.SetRotation(barrelLocal);
                }
            }
        }

        public void SetExtraLightEmissive()
        {
            if (IsActive && extraLightEmissiveIndex != -1)
            {
                nativeVehicle->SetLightEmissive(extraLightEmissiveIndex, Data.ExtraLightEmissive);
            }
        }

        public void SetTrackedEntityAndDisplayNotification(Entity e)
        {
            TrackedEntity = e;
            DisplayTrackedEntityNotification();
        }

        private void DisplayTrackedEntityNotification()
        {
            if (!Plugin.Settings.EnableTrackingNotifications)
            {
                return;
            }

            const string NotificationTitle = "~h~Spotlight";

            Entity e = TrackedEntity;
            if (e)
            {
                string distanceStr = Utility.FormatDistance(Vehicle.DistanceTo(e));
                switch (e)
                {
                    case Vehicle veh:
                        {
                            CVehicle* vehPtr = (CVehicle*)veh.MemoryAddress;
                            IntPtr makeNamePtr = vehPtr->GetMakeName();
                            IntPtr gameNamePtr = vehPtr->GetGameName();
                            string makeName = Utility.IsStringEmpty(makeNamePtr) ? null : Utility.GetLocalizedString(makeNamePtr);
                            string gameName = Utility.IsStringEmpty(gameNamePtr) ? null : Utility.GetLocalizedString(gameNamePtr);
                            string text = $"Model: ~b~{makeName} {gameName}~s~";
                            if (veh.LicensePlateType != LicensePlateType.None)
                            {
                                text += $"~n~License Plate: ~b~{veh.LicensePlate}~s~";
                            }
                            text += $"~n~Distance: ~b~{distanceStr}~s~";


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

                            Game.DisplayNotification(txd, txn, NotificationTitle, "~b~Tracking vehicle", text);
                        }
                        break;
                    case Ped ped:
                        GameFiber.StartNew(() =>
                        {
                            uint headshotHandle = NativeFunction.Natives.RegisterPedheadshot<uint>(ped);
                            int startTime = Environment.TickCount;
                            GameFiber.WaitUntil(() => NativeFunction.Natives.IsPedheadshotReady<bool>(headshotHandle), 10000);
                            string txd = NativeFunction.Natives.GetPedheadshotTxdString<string>(headshotHandle);
                            Game.DisplayNotification(txd, txd, NotificationTitle, "~b~Tracking pedestrian", $"Distance: ~b~{distanceStr}~s~");
                            NativeFunction.Natives.UnregisterPedheadshot<uint>(headshotHandle);
                        });
                        break;
                }
            }
            else
            {
                Game.DisplayNotification("timerbar_sr", "timer_cross", NotificationTitle, "~b~Stopped tracking", String.Empty);
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
