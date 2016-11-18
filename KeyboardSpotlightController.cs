namespace Spotlight
{
    // System
    using System;
    using System.Linq;
    using System.Windows.Forms;

    // RPH
    using Rage;

    internal class KeyboardSpotlightController : SpotlightController
    {
        private static bool keysSet;

        private static Keys moveLeftKey;
        private static Keys moveRightKey;
        private static Keys moveUpKey;
        private static Keys moveDownKey;

        private static Keys trackPedKey;
        private static Keys trackVehicleKey;

        public KeyboardSpotlightController(Spotlight owner) : base(owner)
        {
            if (!keysSet)
            {
                moveLeftKey = Plugin.Settings.GeneralSettingsIniFile.ReadEnum<Keys>("Keyboard", "Move Left", Keys.NumPad4);
                moveRightKey = Plugin.Settings.GeneralSettingsIniFile.ReadEnum<Keys>("Keyboard", "Move Right", Keys.NumPad6);
                moveUpKey = Plugin.Settings.GeneralSettingsIniFile.ReadEnum<Keys>("Keyboard", "Move Up", Keys.NumPad8);
                moveDownKey = Plugin.Settings.GeneralSettingsIniFile.ReadEnum<Keys>("Keyboard", "Move Down", Keys.NumPad2);

                trackPedKey = Plugin.Settings.GeneralSettingsIniFile.ReadEnum<Keys>("Keyboard", "TrackPedKey", Keys.NumPad1);
                trackVehicleKey = Plugin.Settings.GeneralSettingsIniFile.ReadEnum<Keys>("Keyboard", "TrackVehicleKey", Keys.NumPad3);

                keysSet = true;
            }

        }

        bool hasMoved = false;
        Rotator rotationDelta;
        public override void UpdateControls()
        {
            hasMoved = false;
            float pitch = 0.0f, yaw = 0.0f;

            if (Game.IsKeyDownRightNow(moveLeftKey))
            {
                hasMoved = true;
                yaw += Owner.Data.MovementSpeed;
            }
            else if (Game.IsKeyDownRightNow(moveRightKey))
            {
                hasMoved = true;
                yaw -= Owner.Data.MovementSpeed;
            }


            if (Game.IsKeyDownRightNow(moveUpKey))
            {
                hasMoved = true;
                pitch += Owner.Data.MovementSpeed;
            }
            else if (Game.IsKeyDownRightNow(moveDownKey))
            {
                hasMoved = true;
                pitch -= Owner.Data.MovementSpeed;
            }


            if (Game.IsKeyDown(trackVehicleKey))
            {
                if (Owner.IsTrackingVehicle)
                {
                    Owner.TrackedVehicle = null;
                }
                else
                {
                    Vehicle v = World.GetClosestEntity(Owner.Position, 130.0f, GetEntitiesFlags.ConsiderAllVehicles | GetEntitiesFlags.ExcludeEmergencyVehicles | GetEntitiesFlags.ExcludePlayerVehicle) as Vehicle;
                    if (v)
                    {
                        Owner.TrackedVehicle = v;
                    }
                }
            }
            else if (Game.IsKeyDown(trackPedKey))
            {
                if (Owner.IsTrackingPed)
                {
                    Owner.TrackedPed = null;
                }
                else
                {
                    Ped p = World.GetEntities(Game.LocalPlayer.Character.Position, 130.0f, GetEntitiesFlags.ConsiderHumanPeds | GetEntitiesFlags.ExcludePlayerPed)
                                 .Where(x => !((Ped)x).IsInAnyVehicle(false))
                                 .FirstOrDefault() as Ped;
                    if (p)
                    {
                        Owner.TrackedPed = p;
                    }
                }
            }


            rotationDelta = new Rotator(pitch, 0.0f, yaw);
        }


        public override bool GetUpdatedRotationDelta(out Rotator rotation)
        {
            rotation = rotationDelta;
            return hasMoved;
        }
    }
}
