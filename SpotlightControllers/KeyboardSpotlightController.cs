namespace Spotlight.SpotlightControllers
{
    using System.Linq;
    using System.Windows.Forms;
    
    using Rage;

    using Spotlight.Core;

    internal class KeyboardSpotlightController : SpotlightController
    {
        readonly Keys moveLeftKey;
        readonly Keys moveRightKey;
        readonly Keys moveUpKey;
        readonly Keys moveDownKey;

        readonly Keys trackPedKey;
        readonly Keys trackVehicleKey;

        readonly Keys modifierKey;
        readonly Keys toggleKey;

        protected KeyboardSpotlightController()
        {
            moveLeftKey = Plugin.Settings.GeneralSettingsIniFile.ReadEnum<Keys>("Keyboard", "Move Left", Keys.NumPad4);
            moveRightKey = Plugin.Settings.GeneralSettingsIniFile.ReadEnum<Keys>("Keyboard", "Move Right", Keys.NumPad6);
            moveUpKey = Plugin.Settings.GeneralSettingsIniFile.ReadEnum<Keys>("Keyboard", "Move Up", Keys.NumPad8);
            moveDownKey = Plugin.Settings.GeneralSettingsIniFile.ReadEnum<Keys>("Keyboard", "Move Down", Keys.NumPad2);

            trackPedKey = Plugin.Settings.GeneralSettingsIniFile.ReadEnum<Keys>("Keyboard", "TrackPedKey", Keys.NumPad1);
            trackVehicleKey = Plugin.Settings.GeneralSettingsIniFile.ReadEnum<Keys>("Keyboard", "TrackVehicleKey", Keys.NumPad3);

            modifierKey = Plugin.Settings.GeneralSettingsIniFile.ReadEnum<Keys>("Keyboard", "Modifier", Keys.None);
            toggleKey = Plugin.Settings.GeneralSettingsIniFile.ReadEnum<Keys>("Keyboard", "Toggle", Keys.I);
        }

        public override bool ShouldToggleSpotlight() => Utility.IsKeyDownWithModifier(toggleKey, modifierKey);

        bool hasMoved = false;
        Rotator rotationDelta;
        protected override void UpdateControlsInternal(VehicleSpotlight spotlight)
        {
            hasMoved = false;
            float pitch = 0.0f, yaw = 0.0f;

            if (Utility.IsKeyDownRightNowWithModifier(moveLeftKey, modifierKey))
            {
                hasMoved = true;
                yaw += spotlight.Data.MovementSpeed;
            }
            else if (Utility.IsKeyDownRightNowWithModifier(moveRightKey, modifierKey))
            {
                hasMoved = true;
                yaw -= spotlight.Data.MovementSpeed;
            }


            if (Utility.IsKeyDownRightNowWithModifier(moveUpKey, modifierKey))
            {
                hasMoved = true;
                pitch += spotlight.Data.MovementSpeed;
            }
            else if (Utility.IsKeyDownRightNowWithModifier(moveDownKey, modifierKey))
            {
                hasMoved = true;
                pitch -= spotlight.Data.MovementSpeed;
            }


            if (Utility.IsKeyDownWithModifier(trackVehicleKey, modifierKey))
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
            else if (Utility.IsKeyDownWithModifier(trackPedKey, modifierKey))
            {
                if (spotlight.IsTrackingPed)
                {
                    spotlight.TrackedPed = null;
                }
                else
                {
                    Ped p = World.GetEntities(Game.LocalPlayer.Character.Position, 130.0f, GetEntitiesFlags.ConsiderHumanPeds | GetEntitiesFlags.ExcludePlayerPed)
                                 .Where(x => !((Ped)x).IsInAnyVehicle(false))
                                 .OrderBy(x => Vector3.DistanceSquared(x.Position, spotlight.Position))
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
