namespace Spotlight
{
    using System;
    using System.Collections.Generic;
    
    using Rage;

    using Spotlight.Core;
    using Spotlight.Core.Memory;
    using Spotlight.InputControllers;

    internal unsafe class VehicleSpotlight : BaseSpotlight
    {
        private readonly CVehicle* nativeVehicle;

        public Vehicle Vehicle { get; }
        public VehicleData VehicleData { get; }
        
        public Vector3 Offset { get => VehicleData.Offset; }
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
                    if (Vehicle)
                    {
                        CVehicleWeaponMgr* weaponMgr = nativeVehicle->GetWeaponMgr();
                        if (weaponMgr != null)
                        {
                            weaponMgr->GetTurret(weaponMgr->GetWeapon(nativeWeaponIndex)->turretIndex)->baseBoneRefId = nativeTurretBaseBoneRefId;
                            weaponMgr->GetTurret(weaponMgr->GetWeapon(nativeWeaponIndex)->turretIndex)->barrelBoneRefId = nativeTurretBarrelBoneRefId;
                        }
                    }
                }

                base.IsActive = value;
            }
        }

        // turret stuff
        public VehicleBone WeaponBone { get; }
        public VehicleBone TurretBaseBone { get; }
        // barrel is optional
        public VehicleBone TurretBarrelBone { get; }

        private readonly bool enableTurret = false;

        private readonly int nativeWeaponIndex = -1;
        private readonly int nativeTurretBaseBoneRefId = -1;
        private readonly int nativeTurretBarrelBoneRefId = -1;

        public VehicleSpotlight(Vehicle vehicle) : base(GetSpotlightDataForModel(vehicle.Model))
        {
            Vehicle = vehicle;
            nativeVehicle = (CVehicle*)vehicle.MemoryAddress;
            VehicleData = GetVehicleDataForModel(vehicle.Model);
            if (VehicleData.EnableTurret)
            {
                CVehicleWeaponMgr* weaponMgr = nativeVehicle->GetWeaponMgr();
                if(weaponMgr != null)
                {
                    uint searchlightHash = Game.GetHashKey("VEHICLE_WEAPON_SEARCHLIGHT");
                    for (int i = 0; i < weaponMgr->GetMaxWeapons(); i++)
                    {
                        CVehicleWeapon* weapon = weaponMgr->GetWeapon(i);
                        if(weapon != null)
                        {
                            if (weapon->GetName() == searchlightHash)
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

                    if (nativeWeaponIndex != -1)
                    {
                        int turretIndex = weaponMgr->GetWeapon(nativeWeaponIndex)->turretIndex;
                        CTurret* turret = null;
                        if (turretIndex >= 0 && turretIndex < weaponMgr->GetMaxTurrets())
                        {
                            turret = weaponMgr->GetTurret(turretIndex);
                        }

                        if (turret != null)
                        {
                            int nativeWeaponBoneRefId = weaponMgr->GetWeapon(nativeWeaponIndex)->boneRefId;
                            nativeTurretBaseBoneRefId = turret->baseBoneRefId;
                            nativeTurretBarrelBoneRefId = turret->barrelBoneRefId;

                            if (nativeWeaponBoneRefId != -1)
                            {
                                byte i = nativeVehicle->GetBoneRefsArray()[nativeWeaponBoneRefId];
                                if(i != 0xFF)
                                {
                                    int nativeWeaponBoneIndex = i;
                                    if (!VehicleBone.TryGetForVehicle(Vehicle, nativeWeaponBoneIndex, out VehicleBone weaponBone))
                                    {
                                        throw new InvalidOperationException($"The model \"{vehicle.Model.Name}\" doesn't have the bone of index {nativeWeaponBoneIndex} for the CVehicleWeapon Bone");
                                    }
                                    WeaponBone = weaponBone;
                                }
                            }

                            if (nativeTurretBaseBoneRefId != -1)
                            {
                                byte i = nativeVehicle->GetBoneRefsArray()[nativeTurretBaseBoneRefId];
                                if (i != 0xFF)
                                {
                                    int nativeTurretBaseBoneIndex = i;
                                    if (!VehicleBone.TryGetForVehicle(Vehicle, nativeTurretBaseBoneIndex, out VehicleBone baseBone))
                                    {
                                        throw new InvalidOperationException($"The model \"{vehicle.Model.Name}\" doesn't have the bone of index {nativeTurretBaseBoneIndex} for the CTurret Base Bone");
                                    }
                                    TurretBaseBone = baseBone;
                                }
                            }

                            if (nativeTurretBarrelBoneRefId != -1)
                            {
                                byte i = nativeVehicle->GetBoneRefsArray()[nativeTurretBarrelBoneRefId];
                                if (i != 0xFF)
                                {
                                    int nativeTurretBarrelBoneIndex = i;
                                    VehicleBone.TryGetForVehicle(Vehicle, nativeTurretBarrelBoneIndex, out VehicleBone barrelBone);
                                    TurretBarrelBone = barrelBone;
                                }
                            }

                            enableTurret = true;
                        }
                    }
                }
            }

            if (vehicle.Model.IsHelicopter)
                RelativeRotation = new Rotator(-50.0f, 0.0f, 0.0f);
        }

        public void OnUnload()
        {
            if (enableTurret && Vehicle)
            {
                CVehicleWeaponMgr* weaponMgr = nativeVehicle->GetWeaponMgr();
                if (weaponMgr != null)
                {
                    weaponMgr->GetTurret(weaponMgr->GetWeapon(nativeWeaponIndex)->turretIndex)->baseBoneRefId = nativeTurretBaseBoneRefId;
                    weaponMgr->GetTurret(weaponMgr->GetWeapon(nativeWeaponIndex)->turretIndex)->barrelBoneRefId = nativeTurretBarrelBoneRefId;
                }
            }
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
                    weaponMgr->GetTurret(weaponMgr->GetWeapon(nativeWeaponIndex)->turretIndex)->baseBoneRefId = -1;
                    weaponMgr->GetTurret(weaponMgr->GetWeapon(nativeWeaponIndex)->turretIndex)->barrelBoneRefId = -1;
                }
            }

            if (enableTurret)
            {

                Position = MathHelper.GetOffsetPosition(Vehicle.GetBonePosition(WeaponBone.Index), Vehicle.GetBoneOrientation(WeaponBone.Index), VehicleData.Offset);
            }
            else
            {
                Position = Vehicle.GetOffsetPosition(Offset);
            }

            if (IsCurrentPlayerVehicleSpotlight)
            {
                for (int i = 0; i < controllers.Count; i++)
                {
                    controllers[i].UpdateControls(this);
                    if (!IsTrackingVehicle && !IsTrackingPed &&controllers[i].GetUpdatedRotationDelta(this, out Rotator newRotDelta))
                    {
                        RelativeRotation += newRotDelta;
                        break;
                    }
                }
            }


            if (enableTurret)
            {
                // TODO: fix turret rotation
                //  Sometimes for a tick the spotlight has the wrong direction when tracking
                //  and, rarely, the turret bone disappears, probably due to an invalid rotation
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

            DrawLight();
        }

        internal static VehicleData GetVehicleDataForModel(Model model)
        {
            foreach (KeyValuePair<string, VehicleData> entry in Plugin.Settings.Vehicles.Data)
            {
                if (model == new Model(entry.Key))
                    return entry.Value;
            }

            Game.LogTrivial("No spotlight offset position loaded for model: " + model.Name);
            Game.LogTrivial("Using default values");
            return VehicleData.Default;
        }

        //internal static Vector3 GetOffsetForModel(Model model)
        //{
        //    foreach (KeyValuePair<string, VehicleData> entry in Plugin.Settings.Vehicles.Data)
        //    {
        //        if (model == new Model(entry.Key))
        //            return entry.Value.Offset;
        //    }

        //    Game.LogTrivial("No spotlight offset position loaded for model: " + model.Name);
        //    Game.LogTrivial("Using default values");
        //    return new Vector3(-0.8f, 1.17f, 0.52f);
        //}

        internal static SpotlightData GetSpotlightDataForModel(Model model)
        {
            return model.IsBoat ? Plugin.Settings.Visual.Boat :
                   model.IsHelicopter ? Plugin.Settings.Visual.Helicopter : Plugin.Settings.Visual.Default;
        }
    }
}
