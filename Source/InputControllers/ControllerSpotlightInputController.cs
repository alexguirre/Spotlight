namespace Spotlight.InputControllers
{
    using System;

    using Rage;
    using Rage.Native;

    using Spotlight.Core;

    internal class ControllerSpotlightInputController : SpotlightInputController
    {
        private enum ControllerMethod { LeftStick, RightStick, DPad }

        readonly ControllerMethod method;

        readonly ControllerButtons modifierButton;
        readonly ControllerButtons toggleButton;

        protected ControllerSpotlightInputController()
        {
            method = (ControllerMethod)Enum.Parse(typeof(ControllerMethod), Plugin.Settings.GeneralSettingsIniFile.ReadString("Controller", "Method", nameof(ControllerMethod.LeftStick)), true);

            modifierButton = Plugin.Settings.GeneralSettingsIniFile.ReadEnum<ControllerButtons>("Controller", "Modifier", ControllerButtons.LeftShoulder);
            toggleButton = Plugin.Settings.GeneralSettingsIniFile.ReadEnum<ControllerButtons>("Controller", "Toggle", ControllerButtons.X);
        }

        public override bool ShouldToggleSpotlight() => Utility.IsControllerButtonDownWithModifier(toggleButton, modifierButton);

        bool hasMoved = false;
        Rotator rotationDelta;
        protected override void UpdateControlsInternal(VehicleSpotlight spotlight)
        {
            hasMoved = false;
            float pitch = 0.0f, yaw = 0.0f;

            switch (method)
            {
                case ControllerMethod.LeftStick:
                    if (modifierButton == ControllerButtons.None || Game.IsControllerButtonDownRightNow(modifierButton))
                    {
                        yaw -= Utility.GetDisabledControlNormal(GameControl.FrontendAxisX) * spotlight.Data.MovementSpeed;
                        pitch -= Utility.GetDisabledControlNormal(GameControl.FrontendAxisY) * spotlight.Data.MovementSpeed;
                    }
                    break;
                case ControllerMethod.RightStick:
                    if (modifierButton == ControllerButtons.None || Game.IsControllerButtonDownRightNow(modifierButton))
                    {
                        yaw -= Utility.GetDisabledControlNormal(GameControl.FrontendRightAxisX) * spotlight.Data.MovementSpeed;
                        pitch -= Utility.GetDisabledControlNormal(GameControl.FrontendRightAxisY) * spotlight.Data.MovementSpeed;
                    }
                    break;
                case ControllerMethod.DPad:
                    if (modifierButton == ControllerButtons.None || Game.IsControllerButtonDownRightNow(modifierButton))
                    {
                        if (Game.IsControllerButtonDownRightNow(ControllerButtons.DPadLeft))
                            yaw += spotlight.Data.MovementSpeed;
                        else if (Game.IsControllerButtonDownRightNow(ControllerButtons.DPadRight))
                            yaw -= spotlight.Data.MovementSpeed;

                        if (Game.IsControllerButtonDownRightNow(ControllerButtons.DPadUp))
                            pitch += spotlight.Data.MovementSpeed;
                        else if (Game.IsControllerButtonDownRightNow(ControllerButtons.DPadDown))
                            pitch -= spotlight.Data.MovementSpeed;
                    }
                    break;
            }

            if (pitch != 0.0f || yaw != 0.0f)
                hasMoved = true;
            rotationDelta = new Rotator(pitch, 0.0f, yaw);
        }


        protected override bool GetUpdatedRotationDeltaInternal(VehicleSpotlight spotlight, out Rotator rotation)
        {
            rotation = rotationDelta;
            return hasMoved;
        }
    }
}
