namespace Spotlight
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Collections.Generic;
    
    using Rage;
    
    using Spotlight.InputControllers;
    using Spotlight.Editor;

    internal static unsafe class Plugin
    {
        public static Settings Settings { get; private set; }

        public static readonly List<VehicleSpotlight> Spotlights = new List<VehicleSpotlight>();
        public static readonly List<SpotlightInputController> SpotlightInputControllers = new List<SpotlightInputController>();
        
        public static EditorForm Editor { get; private set; }

        private static void Main()
        {
            while (Game.IsLoading)
                GameFiber.Sleep(500);

            if (!Directory.Exists(@"Plugins\Spotlight Resources\"))
                Directory.CreateDirectory(@"Plugins\Spotlight Resources\");

            Settings = new Settings(@"Plugins\Spotlight Resources\General.ini",
                                    @"Plugins\Spotlight Resources\Offsets.ini",
                                    @"Plugins\Spotlight Resources\Spotlight Data - Cars.xml",
                                    @"Plugins\Spotlight Resources\Spotlight Data - Helicopters.xml",
                                    @"Plugins\Spotlight Resources\Spotlight Data - Boats.xml",
                                    true);

            LoadSpotlightControllers();
            
            while (true)
            {
                GameFiber.Yield();
                Update();
            }
        }

        private static void Update()
        {
            if (SpotlightInputControllers.Any(c => c.ShouldToggleSpotlight()))
            {
                VehicleSpotlight s = GetPlayerCurrentVehicleSpotlight();

                if (s != null)
                {
                    s.IsActive = !s.IsActive;
                }
            }


            for (int i = Spotlights.Count - 1; i >= 0; i--)
            {
                VehicleSpotlight s = Spotlights[i];
                if (!s.Vehicle || s.Vehicle.IsDead)
                {
                    s.IsActive = false;
                    Spotlights.Remove(s);
                    continue;
                }

                s.Update(SpotlightInputControllers);
            }

            if ((Editor == null || !Editor.Window.IsVisible) && Game.IsKeyDown(Settings.EditorKey))
            {
                if (Editor != null)
                {
                    Editor?.Window?.Close();
                    Editor = null;
                }

                Editor = new EditorForm();
                Editor.Show();
                Editor.Position = new System.Drawing.Point(300, 300);
            }
        }

        private static void OnUnload(bool isTerminating)
        {
            if (!isTerminating)
            {
                // native calls: delete entities, blips, etc.
            }

            // dispose objects
            Spotlights.Clear();
        }


        private static VehicleSpotlight GetPlayerCurrentVehicleSpotlight()
        {
            Vehicle v = Game.LocalPlayer.Character.CurrentVehicle;
            if (v)
            {
                return GetVehicleSpotlight(v);
            }

            return null;
        }

        private static VehicleSpotlight GetVehicleSpotlight(Vehicle vehicle)
        {
            VehicleSpotlight s = Spotlights.FirstOrDefault(l => l.Vehicle == vehicle);

            if (s != null)
                return s;

            s = new VehicleSpotlight(vehicle);
            Spotlights.Add(s);
            return s;
        }

        private static void LoadSpotlightControllers()
        {
            IEnumerable<Type> types = System.Reflection.Assembly.GetExecutingAssembly().GetTypes();

            foreach (Type type in types)
            {
                if (!type.IsAbstract && !type.IsInterface && typeof(SpotlightInputController).IsAssignableFrom(type))
                {
                    string iniKeyName = type.Name.Replace("SpotlightInputController", "") + "ControlsEnabled";
                    if (Settings.GeneralSettingsIniFile.DoesKeyExist("Controls", iniKeyName) && Settings.GeneralSettingsIniFile.ReadBoolean("Controls", iniKeyName, false))
                    {
                        SpotlightInputController c = (SpotlightInputController)Activator.CreateInstance(type, true);
                        SpotlightInputControllers.Add(c);
                    }
                }
            }
        }
    }
}
