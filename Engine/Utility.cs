namespace Spotlight
{
    // System
    using System.Drawing;

    // RPH
    using Rage;
    using Rage.Native;

    using Engine.Memory;

    internal static class Utility
    {
        public static void DrawSpotlight(ISpotlight spotlight)
        {
            if (!spotlight.IsActive)
                return;

            DrawSpotlight(spotlight.Position, spotlight.Direction, spotlight.Data.Color, spotlight.Data.Shadow, spotlight.Data.Radius, spotlight.Data.Brightness, spotlight.Data.Distance, spotlight.Data.Falloff, spotlight.Data.Roundness);
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

        public static unsafe void DrawCorona(Vector3 position, Vector3 direction, Color color)
        {
            NativeVector3 p = position;
            NativeVector3 d = direction;
            GameFunctions.DrawCorona(GameFunctions.DrawCoronaUnkPtr, &p, 2.25f, unchecked((uint)color.ToArgb()), 80.0f, 100.0f, &d, 1.0f, 10.0f, 65.0f, 2);
        }
    }
}
