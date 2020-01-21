
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
            PluginState.Init();

            while (true)
            {
                GameFiber.Yield();

                Vehicle closest = Game.LocalPlayer.Character.LastVehicle;
                if (!closest)
                {
                    Vehicle[] vehs = Game.LocalPlayer.Character.GetNearbyVehicles(1);
                    if (vehs.Length > 0)
                    {
                        closest = vehs[0];
                    }
                }

                if (closest)
                {
                    bool hasSpotlight = closest.HasSpotlight();
                    Entity tracked = closest.GetSpotlightTrackedEntity();
                    string s = tracked ? tracked.Handle.ToString() : "null";
                    Game.DisplayHelp($"IsLoaded = {PluginState.IsLoaded}~n~Closest~n~Has spotlight = {hasSpotlight}~n~TrackedEntity = {s}");

                    if (Game.IsKeyDown(Keys.J))
                    {
                        Vehicle v = new Vehicle(closest.Position + Vector3.WorldUp * 5.0f);
                        v.Dismiss();
                        closest.SetSpotlightTrackedEntity(v);
                    }
                }
            }
        }

        public static void End(bool isTerminating)
        {
            spotlight1?.Dispose();
            spotlight2?.Dispose();

            PluginState.Shutdown();
        }
    }
}
