namespace Spotlight.InputControllers
{
    using System.Linq;
    using System.Windows.Forms;

    using Rage;

    using Spotlight.Core;

    internal class KeyboardSpotlightInputController : SpotlightInputController
    {
        readonly Keys moveLeftKey;
        readonly Keys moveRightKey;
        readonly Keys moveUpKey;
        readonly Keys moveDownKey;

        readonly Keys toggleTrackingKey;
        readonly Keys searchModeKey;

        readonly Keys modifierKey;
        readonly Keys toggleKey;

        protected KeyboardSpotlightInputController()
        {
            moveLeftKey = Plugin.Settings.GeneralSettingsIniFile.ReadEnum<Keys>("Keyboard", "Move Left", Keys.NumPad4);
            moveRightKey = Plugin.Settings.GeneralSettingsIniFile.ReadEnum<Keys>("Keyboard", "Move Right", Keys.NumPad6);
            moveUpKey = Plugin.Settings.GeneralSettingsIniFile.ReadEnum<Keys>("Keyboard", "Move Up", Keys.NumPad8);
            moveDownKey = Plugin.Settings.GeneralSettingsIniFile.ReadEnum<Keys>("Keyboard", "Move Down", Keys.NumPad2);

            toggleTrackingKey = Plugin.Settings.GeneralSettingsIniFile.ReadEnum<Keys>("Keyboard", "ToggleTrackingKey", Keys.NumPad3);
            searchModeKey = Plugin.Settings.GeneralSettingsIniFile.ReadEnum<Keys>("Keyboard", "SearchModeKey", Keys.Decimal);

            modifierKey = Plugin.Settings.GeneralSettingsIniFile.ReadEnum<Keys>("Keyboard", "Modifier", Keys.None);
            toggleKey = Plugin.Settings.GeneralSettingsIniFile.ReadEnum<Keys>("Keyboard", "Toggle", Keys.I);

            MovementSpeed = Plugin.Settings.GeneralSettingsIniFile.ReadSingle("Keyboard", "Speed", 100.0f);
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
                yaw += GetMovementAmountForThisFrame();
            }
            else if (Utility.IsKeyDownRightNowWithModifier(moveRightKey, modifierKey))
            {
                hasMoved = true;
                yaw -= GetMovementAmountForThisFrame();
            }


            if (Utility.IsKeyDownRightNowWithModifier(moveUpKey, modifierKey))
            {
                hasMoved = true;
                pitch += GetMovementAmountForThisFrame();
            }
            else if (Utility.IsKeyDownRightNowWithModifier(moveDownKey, modifierKey))
            {
                hasMoved = true;
                pitch -= GetMovementAmountForThisFrame();
            }


            if (Utility.IsKeyDownWithModifier(toggleTrackingKey, modifierKey))
            {
                if (spotlight.IsTrackingEntity)
                {
                    spotlight.TrackedEntity = null;
                }
                else
                {
                    Entity e = GetClosestEntityToSpotlight(spotlight, true, true);
                    if (e)
                    {
                        spotlight.TrackedEntity = e;
                    }
                }
            }
            else if (Utility.IsKeyDownWithModifier(searchModeKey, modifierKey))
            {
                spotlight.IsInSearchMode = !spotlight.IsInSearchMode;
            }


            rotationDelta = new Rotator(pitch, 0.0f, yaw);
        }


        protected override bool GetUpdatedRotationDeltaInternal(VehicleSpotlight spotlight, out Rotator rotation)
        {
            rotation = rotationDelta;
            return hasMoved;
        }

        private Entity GetClosestEntityToSpotlight(VehicleSpotlight spotlight, bool peds, bool vehicles)
        {
            HitResult result = World.TraceCapsule(spotlight.Position, spotlight.Position + spotlight.Direction * 2000.0f,
                                                    6.5f,
                                                    (peds ? TraceFlags.IntersectPedsSimpleCollision : TraceFlags.None) | (vehicles ? TraceFlags.IntersectVehicles : TraceFlags.None),
                                                    spotlight.Vehicle);
            if (result.Hit)
            {
                return result.HitEntity;
            }

            return null;
        }
    }
}
