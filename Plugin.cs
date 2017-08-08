namespace Spotlight
{
    // System
    using System;
    using System.IO;
    using System.Linq;
    using System.Collections.Generic;

    // RPH
    using Rage;
    
    using Spotlight.SpotlightControllers;
    using Spotlight.Engine.Memory;

    internal static unsafe class Plugin
    {
        public static Settings Settings { get; private set; }

        public static readonly List<VehicleSpotlight> Spotlights = new List<VehicleSpotlight>();
        public static readonly List<SpotlightController> SpotlightControllers = new List<SpotlightController>();
        
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

        private static void LogLightDrawData(CLightDrawData d)
        {
            Game.Console.Print($"Position: " + (Vector3)d.Position);
            Game.Console.Print($"Color: " + System.Drawing.Color.FromArgb((int)(d.Color.A * 255), (int)(d.Color.R * 255), (int)(d.Color.G * 255), (int)(d.Color.B * 255)));
            Game.Console.Print($"VolumeOuterColor: " + System.Drawing.Color.FromArgb((int)(d.VolumeOuterColor.A * 255), (int)(d.VolumeOuterColor.R * 255), (int)(d.VolumeOuterColor.G * 255), (int)(d.VolumeOuterColor.B * 255)));
            Game.Console.Print($"LightType: " + d.LightType);
            Game.Console.Print($"Flags: " + d.Flags);
            Game.Console.Print($"Brightness: " + d.Brightness);
            Game.Console.Print($"unkTxdDefPoolIndex: " + d.unkTxdDefPoolIndex);
            Game.Console.Print($"VolumeIntensity: " + d.VolumeIntensity);
            Game.Console.Print($"VolumeSize: " + d.VolumeSize);
            Game.Console.Print($"VolumeExponent: " + d.VolumeExponent);
            Game.Console.Print($"Range: " + d.Range);
            Game.Console.Print($"FalloffExponent: " + d.FalloffExponent);
        }

        private static void Update()
        {
            if (SpotlightControllers.Any(c => c.ShouldToggleSpotlight()))
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
                    Spotlights.Remove(s);
                    continue;
                }

                s.Update(SpotlightControllers);
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
                if (!type.IsAbstract && !type.IsInterface && typeof(SpotlightController).IsAssignableFrom(type))
                {
                    string iniKeyName = type.Name.Replace("SpotlightController", "") + "ControlsEnabled";
                    if (Settings.GeneralSettingsIniFile.DoesKeyExist("Controls", iniKeyName) && Settings.GeneralSettingsIniFile.ReadBoolean("Controls", iniKeyName, false))
                    {
                        SpotlightController c = (SpotlightController)Activator.CreateInstance(type, true);
                        SpotlightControllers.Add(c);
                    }
                }
            }
        }
    }
}
