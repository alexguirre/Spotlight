﻿namespace Spotlight
{
    // System
    using System;
    using System.Linq;
    using System.Collections.Generic;
    using System.Drawing;

    // RPH
    using Rage;
    using Rage.Native;

    using SpotlightControllers;

    internal class VehicleSpotlight : BaseSpotlight
    {
        public Vehicle Vehicle { get; }
        
        public Vector3 Offset { get; }
        public Rotator RelativeRotation { get; set; }

        public bool IsTrackingPed { get { return TrackedPed.Exists(); } }
        public Ped TrackedPed { get; set; }

        public bool IsTrackingVehicle { get { return TrackedVehicle.Exists(); } }
        public Vehicle TrackedVehicle { get; set; }
        
        public bool IsCurrentPlayerVehicleSpotlight { get { return Vehicle == Game.LocalPlayer.Character.CurrentVehicle; } }

        public VehicleSpotlight(Vehicle vehicle) : base(GetSpotlightDataForModel(vehicle.Model))
        {
            Vehicle = vehicle;
            Offset = GetOffsetForModel(vehicle.Model);

            if (vehicle.Model.IsHelicopter)
                RelativeRotation = new Rotator(-50.0f, 0.0f, 0.0f);

            Game.FrameRender += OnDrawCoronaFrameRender;
        }

        ~VehicleSpotlight()
        {
            Game.FrameRender -= OnDrawCoronaFrameRender;
        }

        public void Update(IList<SpotlightController> controllers)
        {
            if (!IsActive)
                return;
            
            Position = Vehicle.GetOffsetPosition(Offset);

            bool isCurrentPlayerVehicleSpotlight = IsCurrentPlayerVehicleSpotlight;

            if (isCurrentPlayerVehicleSpotlight)
            {
                for (int i = 0; i < controllers.Count; i++)
                {
                    controllers[i].UpdateControls(this);
                }
            }

            if (IsTrackingVehicle)
            {
                Direction = (TrackedVehicle.Position - Position).ToNormalized();
            }
            else if (IsTrackingPed)
            {
                Direction = (TrackedPed.Position - Position).ToNormalized();
            }
            else
            {
                if (isCurrentPlayerVehicleSpotlight)
                {
                    for (int i = 0; i < controllers.Count; i++)
                    {
                        if (controllers[i].GetUpdatedRotationDelta(this, out Rotator newRotDelta))
                        {
                            RelativeRotation += newRotDelta;
                            break;
                        }
                    }
                }
                Direction = (Vehicle.Rotation + RelativeRotation).ToVector();
            }

            DrawLight();
        }


        private static Vector3 GetOffsetForModel(Model model)
        {
            if (Plugin.Settings.SpotlightOffsets.TryGetValue(model, out Vector3 o))
                return o;
            Game.LogTrivial("No spotlight offset position loaded for model: " + model.Name);
            Game.LogTrivial("Using default values");
            return new Vector3(-0.8f, 1.17f, 0.52f);
        }

        private static SpotlightData GetSpotlightDataForModel(Model model)
        {
            return model.IsCar ? Plugin.Settings.CarsSpotlightData :
                   model.IsBoat ? Plugin.Settings.BoatsSpotlightData :
                   model.IsHelicopter ? Plugin.Settings.HelicoptersSpotlightData : new SpotlightData();
        }
    }
}
