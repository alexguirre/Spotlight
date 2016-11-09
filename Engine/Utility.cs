namespace Spotlight
{
    // System
    using System.Drawing;

    // RPH
    using Rage;
    using Rage.Native;

    internal static class Utility
    {
        public static void DrawSpotlight(Vector3 position, Vector3 direction, SpotlightData data)
        {
            DrawSpotlight(position, direction, data.Color, data.Shadow, data.Radius, data.Brightness, data.Distance, data.Falloff, data.Roundness);
        }

        public static void DrawSpotlight(Vector3 position, Vector3 direction, Color color, bool shadow, float radius, float brightness, float distance, float falloff, float roundness)
        {
            const ulong DrawSpotlightNative = 0x0;
            const ulong DrawSpotlightWithShadowNative = 0x0;

            NativeFunction.CallByHash<uint>(shadow ? DrawSpotlightWithShadowNative : DrawSpotlightNative, 
                                            position.X, position.Y, position.Z,
                                            direction.X, direction.Y, direction.Z,
                                            color.R, color.G, color.B,
                                            distance, brightness, roundness, 
                                            radius, falloff);
        }
    }
}
