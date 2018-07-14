namespace Spotlight.InputControllers
{
    using System.Windows.Forms;

    using Rage;
    using Rage.Native;

    using Spotlight.Core;

    internal class MouseSpotlightInputController : SpotlightInputController
    {
        readonly Keys modifierKey;
        readonly Keys toggleKey;

        protected MouseSpotlightInputController()
        {
            modifierKey = Plugin.Settings.GeneralSettingsIniFile.ReadEnum<Keys>("Mouse", "Modifier", Keys.LControlKey);
            toggleKey = Plugin.Settings.GeneralSettingsIniFile.ReadEnum<Keys>("Mouse", "Toggle", Keys.I);
        }

        public override bool ShouldToggleSpotlight() => Utility.IsKeyDownWithModifier(toggleKey, modifierKey);

        bool hasMoved = false;
        Rotator rotationDelta;
        protected override void UpdateControlsInternal(VehicleSpotlight spotlight)
        {
            hasMoved = false;
            if (modifierKey == Keys.None || Game.IsKeyDownRightNow(modifierKey))
            {
                if (false) // TODO: add option to enable MouseSpotlightInputController old mode
                {
                    float pitch = 0.0f, yaw = 0.0f;

                    Game.DisableControlAction(0, GameControl.LookLeftRight, true);
                    Game.DisableControlAction(0, GameControl.LookUpDown, true);

                    float leftRight = Utility.GetDisabledControlNormal(GameControl.LookLeftRight) * spotlight.Data.MovementSpeed;
                    float upDown = Utility.GetDisabledControlNormal(GameControl.LookUpDown) * spotlight.Data.MovementSpeed;

                    yaw = -leftRight;
                    pitch = -upDown;

                    if (pitch != 0.0f || yaw != 0.0f)
                        hasMoved = true;
                    rotationDelta = new Rotator(pitch, 0.0f, yaw);
                }
                else
                {
                    Rotator camRot = new Rotator(NativeFunction.Natives.GetGameplayCamRelativePitch<float>(), 0.0f, NativeFunction.Natives.GetGameplayCamRelativeHeading<float>());
                    Rotator r = camRot - spotlight.RelativeRotation;
                    if (r != Rotator.Zero)
                    {
                        hasMoved = true;
                    }
                    rotationDelta = r;
                }
            }
        }


        protected override bool GetUpdatedRotationDeltaInternal(VehicleSpotlight spotlight, out Rotator rotation)
        {
            rotation = rotationDelta;
            return hasMoved;
        }
    }
}
