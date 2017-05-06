namespace Spotlight.SpotlightControllers
{
    // System
    using System;
    using System.Linq;
    using System.Windows.Forms;

    // RPH
    using Rage;

    internal class KeyboardSpotlightController : SpotlightController
    {
        private Keys moveLeftKey;
        private Keys moveRightKey;
        private Keys moveUpKey;
        private Keys moveDownKey;

        private Keys trackPedKey;
        private Keys trackVehicleKey;

        protected KeyboardSpotlightController()
        {
            moveLeftKey = Plugin.Settings.GeneralSettingsIniFile.ReadEnum<Keys>("Keyboard", "Move Left", Keys.NumPad4);
            moveRightKey = Plugin.Settings.GeneralSettingsIniFile.ReadEnum<Keys>("Keyboard", "Move Right", Keys.NumPad6);
            moveUpKey = Plugin.Settings.GeneralSettingsIniFile.ReadEnum<Keys>("Keyboard", "Move Up", Keys.NumPad8);
            moveDownKey = Plugin.Settings.GeneralSettingsIniFile.ReadEnum<Keys>("Keyboard", "Move Down", Keys.NumPad2);

            trackPedKey = Plugin.Settings.GeneralSettingsIniFile.ReadEnum<Keys>("Keyboard", "TrackPedKey", Keys.NumPad1);
            trackVehicleKey = Plugin.Settings.GeneralSettingsIniFile.ReadEnum<Keys>("Keyboard", "TrackVehicleKey", Keys.NumPad3);
        }

        bool hasMoved = false;
        Rotator rotationDelta;
        protected override void UpdateControlsInternal(VehicleSpotlight spotlight)
        {
            hasMoved = false;
            float pitch = 0.0f, yaw = 0.0f;

            if (Game.IsKeyDownRightNow(moveLeftKey))
            {
                hasMoved = true;
                yaw += spotlight.Data.MovementSpeed;
            }
            else if (Game.IsKeyDownRightNow(moveRightKey))
            {
                hasMoved = true;
                yaw -= spotlight.Data.MovementSpeed;
            }


            if (Game.IsKeyDownRightNow(moveUpKey))
            {
                hasMoved = true;
                pitch += spotlight.Data.MovementSpeed;
            }
            else if (Game.IsKeyDownRightNow(moveDownKey))
            {
                hasMoved = true;
                pitch -= spotlight.Data.MovementSpeed;
            }


            if (Game.IsKeyDown(trackVehicleKey))
            {
                if (spotlight.IsTrackingVehicle)
                {
                    spotlight.TrackedVehicle = null;
                }
                else
                {
                    Vehicle v = World.GetClosestEntity(spotlight.Position, 130.0f, GetEntitiesFlags.ConsiderAllVehicles | GetEntitiesFlags.ExcludeEmergencyVehicles | GetEntitiesFlags.ExcludePlayerVehicle) as Vehicle;
                    if (v)
                    {
                        spotlight.TrackedVehicle = v;
                    }
                }
            }
            else if (Game.IsKeyDown(trackPedKey))
            {
                if (spotlight.IsTrackingPed)
                {
                    spotlight.TrackedPed = null;
                }
                else
                {
                    Ped p = World.GetEntities(Game.LocalPlayer.Character.Position, 130.0f, GetEntitiesFlags.ConsiderHumanPeds | GetEntitiesFlags.ExcludePlayerPed)
                                 .Where(x => !((Ped)x).IsInAnyVehicle(false))
                                 .FirstOrDefault() as Ped;
                    if (p)
                    {
                        spotlight.TrackedPed = p;
                    }
                }
            }


            rotationDelta = new Rotator(pitch, 0.0f, yaw);
        }


        protected override bool GetUpdatedRotationDeltaInternal(VehicleSpotlight spotlight, out Rotator rotation)
        {
            rotation = rotationDelta;
            return hasMoved;
        }
    }
}
