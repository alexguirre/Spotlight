namespace Spotlight
{
    using System;
    using System.Collections.Generic;
    
    using Rage;

    using Spotlight.InputControllers;

    internal class VehicleSpotlight : BaseSpotlight, IDisposable
    {
        public Vehicle Vehicle { get; }
        
        public Vector3 Offset { get; private set; }
        public Rotator RelativeRotation { get; set; }

        public bool IsTrackingPed { get { return TrackedPed.Exists(); } }
        public Ped TrackedPed { get; set; }

        public bool IsTrackingVehicle { get { return TrackedVehicle.Exists(); } }
        public Vehicle TrackedVehicle { get; set; }
        
        public bool IsCurrentPlayerVehicleSpotlight { get { return Vehicle == Game.LocalPlayer.Character.CurrentVehicle; } }

        public bool IsDisposed { get; private set; }

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
            Game.LogTrivial("VehicleSpotlight not disposed!");
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        
        private void Dispose(bool disposing)
        {
            if (!IsDisposed)
            {
                if (disposing)
                {
                    IsActive = false;
                    Game.FrameRender -= OnDrawCoronaFrameRender;
                }
            }
            IsDisposed = true;
        }

        public void UpdateOffset()
        {
            Offset = GetOffsetForModel(Vehicle.Model);
        }

        public void Update(IList<SpotlightInputController> controllers)
        {
            if (IsDisposed || !IsActive)
                return;
            
            Position = Vehicle.GetOffsetPosition(Offset);

            if (IsCurrentPlayerVehicleSpotlight)
            {
                for (int i = 0; i < controllers.Count; i++)
                {
                    controllers[i].UpdateControls(this);
                    if (!IsTrackingVehicle && !IsTrackingPed &&controllers[i].GetUpdatedRotationDelta(this, out Rotator newRotDelta))
                    {
                        RelativeRotation += newRotDelta;
                        break;
                    }
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
                Direction = (Vehicle.Rotation + RelativeRotation).ToVector();
            }

            DrawLight();
        }


        internal static Vector3 GetOffsetForModel(Model model)
        {
            foreach (KeyValuePair<string, Vector3> entry in Plugin.Settings.SpotlightOffsets)
            {
                if (model == new Model(entry.Key))
                    return entry.Value;
            }

            Game.LogTrivial("No spotlight offset position loaded for model: " + model.Name);
            Game.LogTrivial("Using default values");
            return new Vector3(-0.8f, 1.17f, 0.52f);
        }

        internal static SpotlightData GetSpotlightDataForModel(Model model)
        {
            return model.IsBoat ? Plugin.Settings.Visual.Boat :
                   model.IsHelicopter ? Plugin.Settings.Visual.Helicopter : Plugin.Settings.Visual.Default;
        }
    }
}
