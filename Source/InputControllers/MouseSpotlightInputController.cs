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
                    Vector3 camPos, camDir;
                    if (Camera.RenderingCamera)
                    {
                        Camera cam = Camera.RenderingCamera;
                        camPos = cam.Position;
                        camDir = cam.Direction;
                    }
                    else
                    {
                        camPos = NativeFunction.Natives.xA200EB1EE790F448<Vector3>(); // _GET_GAMEPLAY_CAM_COORDS
                        Vector3 camRotAsVec = NativeFunction.Natives.x5B4E4C817FCC2DFB<Vector3>(2); // _GET_GAMEPLAY_CAM_ROT
                        Rotator camRot = new Rotator(camRotAsVec.X, camRotAsVec.Y, camRotAsVec.Z);
                        camDir = camRot.ToVector();
                    }

                    HitResult result = World.TraceLine(camPos, camPos + camDir * 2000.0f, TraceFlags.IntersectWorld, spotlight.Vehicle);
                    Vector3 targetPosition;
                    if (result.Hit)
                    {
                        targetPosition = result.HitPosition;
                    }
                    else
                    {
                        targetPosition = camPos + camDir * 2000.0f;
                    }

                    Rotator targetRot = (targetPosition - spotlight.Position).ToRotator();
                    Rotator r = targetRot - (spotlight.Vehicle.Rotation + spotlight.RelativeRotation);
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
