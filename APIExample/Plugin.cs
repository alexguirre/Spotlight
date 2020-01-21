
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

            Vehicle spawnedVehicle = null;
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

                if (spawnedVehicle)
                {
                    if (Game.IsKeyDown(Keys.K))
                    {
                        spawnedVehicle.Delete();
                    }
                    else if (Game.IsKeyDown(Keys.L))
                    {
                        spawnedVehicle.SetSpotlightActive(!spawnedVehicle.IsSpotlightActive());
                    }
                    else if (Game.IsKeyDown(Keys.O))
                    {
                        spawnedVehicle.SetSpotlightRotation(Quaternion.FromRotation(new Rotator(45.0f, 0.0f, 45.0f)));
                    }
                }
                else
                {
                    if (Game.IsKeyDown(Keys.K))
                    {
                        Vector3 pos = Game.LocalPlayer.Character.Position + new Vector3(0.0f, 5.0f, 5.0f);
                        spawnedVehicle = new Vehicle((Model m) => m.IsCar, pos);
                        spawnedVehicle.Dismiss();
                        spawnedVehicle.RequestSpotlightAndWait();
                        spawnedVehicle.SetSpotlightActive(true);
                        spawnedVehicle.SetSpotlightTrackedEntity(Game.LocalPlayer.Character);
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
