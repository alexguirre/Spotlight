namespace Spotlight
{
    using System.Drawing;
    using System.Windows.Forms;

    using Rage;
    using Rage.Native;

    using Engine.Memory;

    internal static class Utility
    {
        public static unsafe void DrawSpotlight(ISpotlight spotlight)
        {
            if (!spotlight.IsActive)
                return;

            //DrawSpotlight(, , , spotlight.Data.Shadow, );
            CLightDrawData* drawData = CLightDrawData.New(eLightType.SPOT_LIGHT, 4096, spotlight.Position, spotlight.Data.Color, spotlight.Data.Brightness);
            NativeVector3 dir = spotlight.Direction;
            drawData->Range = spotlight.Data.Distance;
            drawData->VolumeIntensity = 0.3f;
            drawData->VolumeExponent = 70.0f;
            drawData->VolumeSize = 0.1f;
            drawData->VolumeOuterColor = new NativeColorRGBAFloat { R = 1f, G = 0f, B = 0f, A = 1f };
            drawData->FalloffExponent = spotlight.Data.Falloff;
            NativeVector3 u = Vector3.WorldUp; // seems to be the direction rotated 90 degrees
            GameFunctions.SetLightDrawDataDirection(drawData, &dir, &u);
            GameFunctions.SetLightDrawDataRoundnessAndRadius(drawData, spotlight.Data.Roundness, spotlight.Data.Radius);

            unsafe
            {
                // wtf? why calling the wrapper method Utility.DrawCorona crashes, but calling it directly it doesn't?
                // and apparently, now I can call it from a normal gamefiber too, no need for the FrameRender
                NativeVector3 p = spotlight.Position;
                NativeVector3 d = spotlight.Direction;
                GameFunctions.DrawCorona(GameFunctions.DrawCoronaUnkPtr, &p, 2.25f, unchecked((uint)spotlight.Data.Color.ToArgb()), 80.0f, 100.0f, &d, 1.0f, 5.0f, spotlight.Data.Radius, 3);
            }
        }

        public static void DrawSpotlight(Vector3 position, Vector3 direction, Color color, bool shadow, float radius, float brightness, float distance, float falloff, float roundness)
        {
            const ulong DrawSpotlightNative = 0xd0f64b265c8c8b33;
            const ulong DrawSpotlightWithShadowNative = 0x5bca583a583194db;

            NativeFunction.CallByHash<uint>(shadow ? DrawSpotlightWithShadowNative : DrawSpotlightNative, 
                                            position.X, position.Y, position.Z,
                                            direction.X, direction.Y, direction.Z,
                                            color.R, color.G, color.B,
                                            distance, brightness, roundness, 
                                            radius, falloff, shadow ? 0.0f : 0);
        }

        public static bool IsKeyDownWithModifier(Keys key, Keys modifier)
        {
            return modifier == Keys.None ? Game.IsKeyDown(key) : (Game.IsKeyDownRightNow(modifier) && Game.IsKeyDown(key));
        }

        public static bool IsKeyDownRightNowWithModifier(Keys key, Keys modifier)
        {
            return modifier == Keys.None ? Game.IsKeyDownRightNow(key) : (Game.IsKeyDownRightNow(modifier) && Game.IsKeyDownRightNow(key));
        }

        public static bool IsControllerButtonDownWithModifier(ControllerButtons button, ControllerButtons modifier)
        {
            return modifier == ControllerButtons.None ? Game.IsControllerButtonDown(button) : (Game.IsControllerButtonDownRightNow(modifier) && Game.IsControllerButtonDown(button));
        }

        public static bool IsControllerButtonDownRightNowWithModifier(ControllerButtons button, ControllerButtons modifier)
        {
            return modifier == ControllerButtons.None ? Game.IsControllerButtonDownRightNow(button) : (Game.IsControllerButtonDownRightNow(modifier) && Game.IsControllerButtonDownRightNow(button));
        }
    }
}
