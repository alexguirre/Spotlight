namespace Spotlight
{
    using System;
    using System.Linq;
    using System.Collections.Generic;
    
    using Rage;

    using Spotlight.Core;
    using Spotlight.Core.Memory;
    using Spotlight.InputControllers;

    internal unsafe class VehicleSpotlight : BaseSpotlight
    {
        private const eBoneRefId InvalidBoneRefId = (eBoneRefId)(-1);
        private const uint VehicleWeaponSearchlightHash = 0xCDAC517D; // VEHICLE_WEAPON_SEARCHLIGHT

        private readonly CVehicle* nativeVehicle;

        public Vehicle Vehicle { get; }
        public VehicleData VehicleData { get; }
        
        public Rotator RelativeRotation { get; set; }

        public bool IsTrackingPed { get { return TrackedPed.Exists(); } }
        public Ped TrackedPed { get; set; }

        public bool IsTrackingVehicle { get { return TrackedVehicle.Exists(); } }
        public Vehicle TrackedVehicle { get; set; }
        
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

        // turret stuff
        public VehicleBone WeaponBone { get; private set; }
        public VehicleBone TurretBaseBone { get; private set; }
        // barrel is optional
        public VehicleBone TurretBarrelBone { get; private set; }

        private bool enableTurret = false;

        private int nativeWeaponIndex = -1;
        private int nativeTurretIndex = -1;
        private eBoneRefId nativeTurretBaseBoneRefId = InvalidBoneRefId, nativeTurretBarrelBoneRefId = InvalidBoneRefId;

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

                            enableTurret = true;

                            if (!Plugin.Settings.Vehicles.Data.ContainsValue(VehicleData))
                            {
                                VehicleData.Offset = new XYZ(0.0f, 0.0f, 0.0f);
                            }
                        }
                    }
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
                    if (!IsTrackingVehicle && !IsTrackingPed && controllers[i].GetUpdatedRotationDelta(this, out Rotator newRotDelta))
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

                Quaternion q = Quaternion.Identity;
                if (IsTrackingVehicle)
                {
                    Vector3 dir = (TrackedVehicle.Position - Position).ToNormalized();
                    q = (dir.ToQuaternion() * Quaternion.Invert(Vehicle.Orientation));
                }
                else if (IsTrackingPed)
                {
                    Vector3 dir = (TrackedPed.Position - Position).ToNormalized();
                    q = (dir.ToQuaternion() * Quaternion.Invert(Vehicle.Orientation));
                }
                else
                {
                    q = RelativeRotation.ToQuaternion();
                }

                q.Normalize();

                CVehicleWeaponMgr* weaponMgr = nativeVehicle->GetWeaponMgr();
                if (weaponMgr != null)
                {
                    // changes turret rotation so when deactivating the spotlight
                    // the game code doesn't reset the turret bone rotation
                    CTurret* turret = weaponMgr->GetTurret(nativeTurretIndex);
                    turret->rot1 = q;
                    turret->rot2 = q;
                    turret->rot3 = q;
                }

                if (TurretBarrelBone == null)
                {
                    TurretBaseBone.SetRotation(q);

                    Matrix m = Matrix.Multiply(TurretBaseBone.Transform, *nativeVehicle->inst->archetype->skeleton->entityTransform);
                    m.Decompose(out _, out q, out _);

                    Direction = q.ToVector();
                }
                else
                {

                    Rotator rot = q.ToRotation();
                    TurretBaseBone.SetRotation(new Rotator(0.0f, 0.0f, rot.Yaw).ToQuaternion());
                    TurretBarrelBone.SetRotation(new Rotator(rot.Pitch, 0.0f, 0.0f).ToQuaternion());

                    Matrix m = Matrix.Multiply(TurretBaseBone.Transform, *nativeVehicle->inst->archetype->skeleton->entityTransform);
                    m = Matrix.Multiply(TurretBarrelBone.Transform, m);
                    m.Decompose(out _, out q, out _);

                    Direction = q.ToVector();
                }
            }
            else
            {
                Position = Vehicle.GetOffsetPosition(VehicleData.Offset);

                if (IsTrackingVehicle)
                {
                    Direction = (TrackedVehicle.Position - Position).ToNormalized();
                }
                else if (IsTrackingPed)
                {
                    Direction = (TrackedPed.Position - Position).ToNormalized();
                }
                else
                {
                    Direction = (Vehicle.Rotation + RelativeRotation).ToVector();
                }
            }

            justActivated = false;

            DrawLight();
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
