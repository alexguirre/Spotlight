namespace Spotlight.InputControllers
{
    using System;
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

            MovementSpeed = Plugin.Settings.GeneralSettingsIniFile.ReadSingle("Mouse", "Speed", 100.0f);
        }

        public override bool ShouldToggleSpotlight() => Utility.IsKeyDownWithModifier(toggleKey, modifierKey);

        protected override void UpdateControlsInternal(VehicleSpotlight spotlight)
        {
            if (modifierKey == Keys.None || Game.IsKeyDownRightNow(modifierKey))
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
                    Vector3 camRotAsVec = NativeFunction.Natives.x5B4E4C817FCC2DFB<Vector3>(0); // _GET_GAMEPLAY_CAM_ROT
                    Rotator camRot = new Rotator(camRotAsVec.X, camRotAsVec.Y, camRotAsVec.Z);
                    camDir = camRot.ToVector();
                }

                Vector3 end = camPos + camDir * 2000.0f;
                HitResult result = World.TraceLine(camPos, end, TraceFlags.IntersectWorld, spotlight.Vehicle);
                Vector3 targetPosition;
                if (result.Hit)
                {
                    targetPosition = result.HitPosition;
                }
                else
                {
                    targetPosition = end;
                }

                Vector3 worldDir = (targetPosition - spotlight.Position).ToNormalized();
                Quaternion worldRot = worldDir.ToQuaternion();
                Quaternion r = worldRot * Quaternion.Invert(spotlight.Vehicle.Orientation);
                if (MovementSpeed == 0.0f)
                {
                    spotlight.RelativeRotation = r;
                }
                else
                {
                    Quaternion diff = r * Quaternion.Invert(spotlight.RelativeRotation);
                    float diffAngle = MathHelper.ConvertRadiansToDegrees(diff.Angle);
                    if (diffAngle >= 180.0f)
                    {
                        diffAngle = 360.0f - diffAngle;
                    }

                    if (diffAngle > float.Epsilon)
                    {
                        float secondsToFinish = diffAngle / MovementSpeed;
                        float t = Math.Min(1.0f, Game.FrameTime / secondsToFinish);
                        spotlight.RelativeRotation = Quaternion.Slerp(spotlight.RelativeRotation, r, t);
                    }
                }
            }
        }

        protected override bool GetUpdatedRotationDeltaInternal(VehicleSpotlight spotlight, out Rotator rotation)
        {
            rotation = Rotator.Zero;
            return false;
        }
    }
}
