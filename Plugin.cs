namespace Spotlight
{
    // System
    using System;
    using System.IO;
    using System.Threading;
    using System.Windows.Forms;
    using System.Collections.Generic;

    // RPH
    using Rage;

    internal static class Plugin
    {
        public static Settings Settings { get; private set; }

        public static readonly Dictionary<Vehicle, Spotlight> SpotlightsByVehicle = new Dictionary<Vehicle, Spotlight>();

        private static List<Vehicle> vehicleKeysToRemove = new List<Vehicle>();

        private static void Main()
        {
            while (Game.IsLoading)
                GameFiber.Sleep(500);

            if (!Directory.Exists(@"Plugins\Spotlight Resources\"))
                Directory.CreateDirectory(@"Plugins\Spotlight Resources\");

            Settings = new Settings(@"Plugins\Spotlight Resources\general.ini",
                                    @"Plugins\Spotlight Resources\offsets.ini",
                                    @"Plugins\Spotlight Resources\cars spotlight data.xml",
                                    @"Plugins\Spotlight Resources\helicopters spotlight data.xml",
                                    @"Plugins\Spotlight Resources\boats spotlight data.xml",
                                    true);

            while (true)
            {
                GameFiber.Yield();

                Update();
            }
        }

        private static void Update()
        {
            if (Game.IsKeyDown(Settings.ToggleSpotlightKey))
            {
                Spotlight s = GetPlayerCurrentVehicleSpotlight();

                if (s != null)
                {
                    Game.DisplaySubtitle(s.IsActive.ToString());
                    s.IsActive = !s.IsActive;
                }
            }


            foreach (KeyValuePair<Vehicle, Spotlight> p in SpotlightsByVehicle)
            {
                if (!p.Key.Exists() || p.Key.IsDead)
                {
                    vehicleKeysToRemove.Add(p.Key);
                    continue;
                }

                p.Value.Update();
            }


            for (int i = 0; i < vehicleKeysToRemove.Count; i++)
            {
                SpotlightsByVehicle.Remove(vehicleKeysToRemove[i]);
            }
            vehicleKeysToRemove.Clear();
        }

        private static void OnUnload(bool isTerminating)
        {
            if (!isTerminating)
            {
                // native calls: delete entities, blips, etc.

            }

            // dispose objects
            SpotlightsByVehicle.Clear();
            vehicleKeysToRemove.Clear();
        }


        private static Spotlight GetPlayerCurrentVehicleSpotlight()
        {
            Vehicle v = Game.LocalPlayer.Character.CurrentVehicle;
            if (v)
            {
                return GetVehicleSpotlight(v);
            }

            return null;
        }

        private static Spotlight GetVehicleSpotlight(Vehicle vehicle)
        {
            if (SpotlightsByVehicle.ContainsKey(vehicle))
                return SpotlightsByVehicle[vehicle];

            Spotlight s = new Spotlight(vehicle);
            s.RegisterController<KeyboardSpotlightController>();
            SpotlightsByVehicle.Add(vehicle, s);
            return s;
        }
    }
}

/* RUN WINDOWS FORM
EditSettingsForm = new EditSettingsForm();
FormsThread = new Thread(() =>
{
    Application.EnableVisualStyles();
    Application.Run(EditSettingsForm);
});
FormsThread.SetApartmentState(ApartmentState.STA);
FormsThread.Start();
*/
