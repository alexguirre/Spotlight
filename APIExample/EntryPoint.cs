[assembly: Rage.Attributes.Plugin("Spotlight API Example", Author = "alexguirre", PrefersSingleInstance = true)]
namespace SpotlightAPIExample
{
    using Rage;
    using Rage.Attributes;

    internal static class EntryPoint
    {
        public static void Main()
        {
            GameFiber.Hibernate();
        }

        [ConsoleCommand("Spawn a random car, gives it a spotlight and starts tracking the player")]
        private static void SpawnCarWithSpotlight()
        {
            if (SpotlightAPI.CanBeUsed)
            {
                Vehicle v = new Vehicle((m) => m.IsCar, Game.LocalPlayer.Character.GetOffsetPositionFront(5.0f));
                v.Dismiss();

                // request the Spotlight plugin to give the car a spotlight
                v.RequestSpotlightAndWait();

                // configure the spotlight
                v.SetSpotlightTrackedEntity(Game.LocalPlayer.Character);
                v.SetSpotlightActive(true);
            }
            else
            {
                Game.Console.Print("Spotlight plugin is not loaded");
            }
        }

        [ConsoleCommand("Prints info about the spotlight in the player's current vehicle")]
        private static void PrintSpotlightInfo()
        {
            if (SpotlightAPI.CanBeUsed)
            {
                Vehicle v = Game.LocalPlayer.Character.CurrentVehicle;
                if (v)
                {
                    Game.Console.Print($"Has spotlight\t= {v.HasSpotlight()}");
                    Game.Console.Print($"Is active\t= {v.IsSpotlightActive()}");
                    Game.Console.Print($"Tracked entity\t= {v.GetSpotlightTrackedEntity()?.Handle.ToString() ?? "none"}");
                    Game.Console.Print($"Is in search mode\t= {v.IsSpotlightInSearchMode()}");
                    Game.Console.Print($"Rotation\t= {v.GetSpotlightRotation()}");
                }
                else
                {
                    Game.Console.Print("Not in a vehicle");
                }
            }
            else
            {
                Game.Console.Print("Spotlight plugin is not loaded");
            }
        }

        [ConsoleCommand("Draws a line from the spotlight with the same direction until the spotlight is turn off")]
        private static void DrawSpotlightDirection()
        {
            if (SpotlightAPI.CanBeUsed)
            {
                Vehicle v = Game.LocalPlayer.Character.CurrentVehicle;
                if (v)
                {
                    if (v.HasSpotlight() && v.IsSpotlightActive())
                    {
                        GameFiber.StartNew(() =>
                        {
                            while (v.IsSpotlightActive())
                            {
                                Quaternion relativeRot = v.GetSpotlightRotation();
                                Vector3 worldDir = (relativeRot * v.Orientation).ToVector();
                                Vector3 worldPos = v.GetSpotlightPosition();

                                Debug.DrawLine(worldPos, worldPos + worldDir * 10.0f, System.Drawing.Color.Red);

                                GameFiber.Yield();
                            }
                        });

                    }
                    else
                    {
                        Game.Console.Print("Spotlight is not active");
                    }
                }
                else
                {
                    Game.Console.Print("Not in a vehicle");
                }
            }
            else
            {
                Game.Console.Print("Spotlight plugin is not loaded");
            }
        }
    }
}
