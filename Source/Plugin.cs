namespace Spotlight
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Diagnostics;
    using System.Threading;
    using System.Collections.Generic;
    
    using Rage;

    using Spotlight.Core.Memory;
    using Spotlight.InputControllers;
    using Spotlight.Editor;

    internal static unsafe class Plugin
    {
        public static Settings Settings { get; private set; }

        public static readonly List<VehicleSpotlight> Spotlights = new List<VehicleSpotlight>();
        public static readonly List<SpotlightInputController> InputControllers = new List<SpotlightInputController>();
        
        public static EditorForm Editor { get; private set; }
        
        private static void Main()
        {
            while (Game.IsLoading)
                GameFiber.Sleep(500);

            if (!Directory.Exists(@"Plugins\Spotlight Resources\"))
                Directory.CreateDirectory(@"Plugins\Spotlight Resources\");
            
            // let's keep using the Offsets.ini file for now
            //string vehSettingsFile = @"Plugins\Spotlight Resources\VehiclesSettings.xml";
            //if (!File.Exists(vehSettingsFile) && File.Exists(@"Plugins\Spotlight Resources\Offsets.ini"))
            //{
            //    // legacy
            //    vehSettingsFile = @"Plugins\Spotlight Resources\Offsets.ini";
            //}

            Settings = new Settings(@"Plugins\Spotlight Resources\General.ini",
                                    @"Plugins\Spotlight Resources\Offsets.ini",
                                    @"Plugins\Spotlight Resources\VisualSettings.xml",
                                    true);

            LoadSpotlightControllers();

            if (!(GameFunctions.Init() && GameMemory.Init() && GameOffsets.Init()))
            {
                Game.DisplayNotification($"~r~[ERROR] Spotlight: ~s~Failed to initialize, unloading...");
                Game.LogTrivial($"[ERROR] Failed to initialize, unloading...");
                Game.UnloadActivePlugin();
            }

            if (Settings.EnableLightEmissives)
            {
                VehiclesUpdateHook.Hook();
            }

            // when the queue array that the GetFreeLightDrawDataSlotFromQueue function accesses is full,
            // it uses the TLS to get an allocator to allocate memory for a bigger array,
            // therefore we copy the allocator pointers from the main thread TLS to our current thread TLS.
            WinFunctions.CopyTlsValues(WinFunctions.GetProcessMainThreadId(), WinFunctions.GetCurrentThreadId(), GameOffsets.TlsAllocator0, GameOffsets.TlsAllocator1, GameOffsets.TlsAllocator2);
            
            if (Settings.EnableLightEmissives)
            {
                // TODO: find something better than this vehicles update hook to override the extralight emissives values
                // This function may execute multiple times per tick, which is not optimal
                VehiclesUpdateHook.VehiclesUpdate += OnVehiclesUpdate;
            }

#if DEBUG
            bool f = false;
#endif
            while (true)
            {
                GameFiber.Yield();

#if DEBUG
                if (Game.LocalPlayer.Character.CurrentVehicle)
                {
                    if (Game.IsKeyDown(System.Windows.Forms.Keys.Y))
                    {
                        Game.LocalPlayer.Character.CurrentVehicle.IsPositionFrozen = f = !f;
                    }
                    else if (Game.IsKeyDown(System.Windows.Forms.Keys.D7))
                    {
                        Game.LocalPlayer.Character.CurrentVehicle.Rotation = new Rotator(45.0f, 0.0f, 0.0f);
                    }
                    else if (Game.IsKeyDown(System.Windows.Forms.Keys.D8))
                    {
                        Game.LocalPlayer.Character.CurrentVehicle.Rotation = new Rotator(0.0f, 45.0f, 0.0f);
                    }
                    else if (Game.IsKeyDown(System.Windows.Forms.Keys.D9))
                    {
                        Game.LocalPlayer.Character.CurrentVehicle.Rotation = new Rotator(0.0f, 0.0f, 45.0f);
                    }
                    else if (Game.IsKeyDown(System.Windows.Forms.Keys.D0))
                    {
                        Game.LocalPlayer.Character.CurrentVehicle.Rotation = Rotator.Zero;
                    }
                }
#endif

                Update();
            }
        }

        private static void Update()
        {
            if (Core.Utility.IsPauseMenuActive)
            {
                // with ShouldTickInPauseMenu set the spotlight is still visible in the pause menu 
                // without needing to do anything else, so if the pause menu is active don't do anything
                return;
            }


            if (InputControllers.Any(c => c.ShouldToggleSpotlight()))
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
                    Spotlights.RemoveAt(i);
                    continue;
                }

                s.Update(InputControllers);
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

        private static void OnVehiclesUpdate()
        {
            for (int i = Spotlights.Count - 1; i >= 0; i--)
            {
                Spotlights[i].SetExtraLightEmissive();
            }
        }

        private static void OnUnload(bool isTerminating)
        {
            if (Settings != null && Settings.EnableLightEmissives)
            {
                VehiclesUpdateHook.Unhook();
            }

            for (int i = Spotlights.Count - 1; i >= 0; i--)
            {
                Spotlights[i].OnUnload();
            }
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
                        InputControllers.Add(c);
                    }
                }
            }
        }
    }
}
