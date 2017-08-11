
namespace SpotlightAPIExample
{
    using System;
    using System.Drawing;
    using System.Windows.Forms;

    using Rage;

    using Spotlight;
    using Spotlight.API;

    internal static class Plugin
    {
        static APISpotlight spotlight1;
        static APISpotlight spotlight2;
        static float h;
        public static void Run()
        {
            spotlight1 = new APISpotlight(new SpotlightData(Color.FromArgb(255, 240, 5, 5), true, 45f, 30f, 25f, 30f, 50f, 0.025f, 0.05f, 25f, 1.75f, true, true, true, 480)) { IsActive = true };
            spotlight2 = new APISpotlight(new SpotlightData(Color.FromArgb(255, 5, 5, 240), true, 45f, 30f, 25f, 30f, 50f, 0.025f, 0.05f, 25f, 1.75f, true, true, true, 480)) { IsActive = true };

            while (true)
            {
                GameFiber.Yield();

                h = MathHelper.NormalizeHeading(h + spotlight1.Data.MovementSpeed * Game.FrameTime);
                float h2 = MathHelper.NormalizeHeading(h + 180.0f);

                spotlight1.Position = Game.LocalPlayer.Character.GetOffsetPositionUp(0.95f);
                spotlight2.Position = spotlight1.Position;

                spotlight1.Direction = MathHelper.ConvertHeadingToDirection(h);
                spotlight2.Direction = MathHelper.ConvertHeadingToDirection(h2);
            }
        }

        public static void End( bool isTerminating)
        {
            spotlight1?.Dispose();
            spotlight2?.Dispose();
        }
    }
}
