namespace Spotlight.SpotlightControllers
{
    using System;

    using Rage;
    using Rage.Native;

    using Spotlight.Core;

    internal class ControllerSpotlightController : SpotlightController
    {
        private enum ControllerMethod { LeftStick, RightStick, DPad }

        private readonly ControllerMethod method;

        private readonly ControllerButtons modifierButton;
        private readonly ControllerButtons toggleButton;

        protected ControllerSpotlightController()
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
                        yaw -= NativeFunction.Natives.GetDisabledControlNormal<float>(0, 195) * spotlight.Data.MovementSpeed;
                        pitch -= NativeFunction.Natives.GetDisabledControlNormal<float>(0, 196) * spotlight.Data.MovementSpeed;
                    }
                    break;
                case ControllerMethod.RightStick:
                    if (modifierButton == ControllerButtons.None || Game.IsControllerButtonDownRightNow(modifierButton))
                    {
                        yaw -= NativeFunction.Natives.GetDisabledControlNormal<float>(0, 197) * spotlight.Data.MovementSpeed;
                        pitch -= NativeFunction.Natives.GetDisabledControlNormal<float>(0, 198) * spotlight.Data.MovementSpeed;
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
