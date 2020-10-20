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

            MovementSpeed = Plugin.Settings.GeneralSettingsIniFile.ReadSingle("Controller", "Speed", 100.0f);
        }

        public override bool ShouldToggleSpotlight() => Utility.IsControllerButtonDownWithModifier(toggleButton, modifierButton);

        bool hasMoved = false;
        Rotator rotationDelta;
        protected override void UpdateControlsInternal(VehicleSpotlight spotlight)
        {
            hasMoved = false;
            float pitch = 0.0f, yaw = 0.0f;

            if (modifierButton == ControllerButtons.None || Game.IsControllerButtonDownRightNow(modifierButton))
            {
                switch (method)
                {
                    case ControllerMethod.LeftStick:
                        yaw -= Utility.GetDisabledControlNormal(GameControl.FrontendAxisX) * GetMovementAmountForThisFrame();
                        pitch -= Utility.GetDisabledControlNormal(GameControl.FrontendAxisY) * GetMovementAmountForThisFrame();
                        break;
                    case ControllerMethod.RightStick:
                        yaw -= Utility.GetDisabledControlNormal(GameControl.FrontendRightAxisX) * GetMovementAmountForThisFrame();
                        pitch -= Utility.GetDisabledControlNormal(GameControl.FrontendRightAxisY) * GetMovementAmountForThisFrame();
                        break;
                    case ControllerMethod.DPad:
                        if (Game.IsControllerButtonDownRightNow(ControllerButtons.DPadLeft))
                            yaw += GetMovementAmountForThisFrame();
                        else if (Game.IsControllerButtonDownRightNow(ControllerButtons.DPadRight))
                            yaw -= GetMovementAmountForThisFrame();

                        if (Game.IsControllerButtonDownRightNow(ControllerButtons.DPadUp))
                            pitch += GetMovementAmountForThisFrame();
                        else if (Game.IsControllerButtonDownRightNow(ControllerButtons.DPadDown))
                            pitch -= GetMovementAmountForThisFrame();
                        break;
                }

                if (modifierButton != ControllerButtons.None)
                {
                    Disable(ButtonToScriptControl(modifierButton));
                    switch (method)
                    {
                        case ControllerMethod.LeftStick:
                            Disable(GameControl.ScriptLeftAxisX);
                            Disable(GameControl.ScriptLeftAxisY); 
                            break;
                        case ControllerMethod.RightStick:
                            Disable(GameControl.ScriptRightAxisX);
                            Disable(GameControl.ScriptRightAxisY);
                            break;
                        case ControllerMethod.DPad:
                            Disable(ButtonToScriptControl(ControllerButtons.DPadUp));
                            Disable(ButtonToScriptControl(ControllerButtons.DPadDown));
                            Disable(ButtonToScriptControl(ControllerButtons.DPadLeft));
                            Disable(ButtonToScriptControl(ControllerButtons.DPadRight));
                            break;
                    }
                }
            }

            if (pitch != 0.0f || yaw != 0.0f)
                hasMoved = true;
            rotationDelta = new Rotator(pitch, 0.0f, yaw);
        }

        private static void Disable(GameControl? control)
        {
            if (control.HasValue)
            {
                NativeFunction.Natives.xEDE476E5EE29EDB1(0, (int)control.Value); // SET_INPUT_EXCLUSIVE
                Game.DisableControlAction(0, control.Value, true);
            }
        }

        private static GameControl? ButtonToScriptControl(ControllerButtons button)
        {
            switch (button)
            {
                case ControllerButtons.DPadUp: return GameControl.ScriptPadUp;
                case ControllerButtons.DPadDown: return GameControl.ScriptPadDown;
                case ControllerButtons.DPadLeft: return GameControl.ScriptPadLeft;
                case ControllerButtons.DPadRight: return GameControl.ScriptPadRight;
                case ControllerButtons.Start: return null;
                case ControllerButtons.Back: return null;
                case ControllerButtons.LeftThumb: return GameControl.ScriptLT;
                case ControllerButtons.RightThumb: return GameControl.ScriptRT;
                case ControllerButtons.LeftShoulder: return GameControl.ScriptLB;
                case ControllerButtons.RightShoulder: return GameControl.ScriptRB;
                case ControllerButtons.A: return GameControl.ScriptRDown;
                case ControllerButtons.B: return GameControl.ScriptRRight;
                case ControllerButtons.X: return GameControl.ScriptRLeft;
                case ControllerButtons.Y: return GameControl.ScriptRUp;
                default: return null;
            };
        }

        protected override bool GetUpdatedRotationDeltaInternal(VehicleSpotlight spotlight, out Rotator rotation)
        {
            rotation = rotationDelta;
            return hasMoved;
        }
    }
}
