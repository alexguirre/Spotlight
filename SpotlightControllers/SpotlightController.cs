namespace Spotlight.SpotlightControllers
{
    // RPH
    using Rage;
    
    internal abstract class SpotlightController
    {
        protected VehicleSpotlight LastUpdatedSpotlight { get; private set; }

        protected SpotlightController()
        {
        }

        public abstract bool ShouldToggleSpotlight();

        public void UpdateControls(VehicleSpotlight spotlight)
        {
            LastUpdatedSpotlight = spotlight;
            UpdateControlsInternal(spotlight);
        }

        protected abstract void UpdateControlsInternal(VehicleSpotlight spotlight);


        public bool GetUpdatedRotationDelta(VehicleSpotlight spotlight, out Rotator rotation)
        {
            if (LastUpdatedSpotlight != spotlight)
                throw new System.InvalidOperationException();

            bool result = GetUpdatedRotationDeltaInternal(spotlight, out rotation);
            LastUpdatedSpotlight = null;
            return result;
        }

        protected abstract bool GetUpdatedRotationDeltaInternal(VehicleSpotlight spotlight, out Rotator rotation);
    }
}
