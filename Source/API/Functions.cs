namespace Spotlight.API
{
    using Rage;

    public static class Functions
    {
        private static readonly StaticFinalizer finalizer;

        static Functions()
        {
            PluginState.Init();

            finalizer = new StaticFinalizer(PluginState.Shutdown);
        }

        /// <summary>
        /// Gets whether the Spotlight plugin has been loaded by the user.
        /// </summary>
        public static bool IsLoaded => PluginState.IsLoaded;

        public static bool HasSpotlight(Vehicle vehicle) => PluginState.HasSpotlight(vehicle);

        public static bool RequestSpotlight(Vehicle vehicle) => PluginState.RequestSpotlight(vehicle);

        public static void RequestSpotlightAndWait(Vehicle vehicle) => PluginState.RequestSpotlightAndWait(vehicle);

        public static void SetSpotlightTrackedEntity(Vehicle vehicle, Entity entity) => PluginState.SetSpotlightTrackedEntity(vehicle, entity);

        public static Entity GetSpotlightTrackedEntity(Vehicle vehicle) => PluginState.GetSpotlightTrackedEntity(vehicle);

        public static void SetSpotlightActive(Vehicle vehicle, bool active) => PluginState.SetSpotlightActive(vehicle, active);

        public static bool IsSpotlightActive(Vehicle vehicle) => PluginState.IsSpotlightActive(vehicle);

        public static void SetSpotlightInSearchMode(Vehicle vehicle, bool searchModeActive) => PluginState.SetSpotlightInSearchMode(vehicle, searchModeActive);

        public static bool IsSpotlightInSearchMode(Vehicle vehicle) => PluginState.IsSpotlightInSearchMode(vehicle);

        public static void SetSpotlightRotation(Vehicle vehicle, Quaternion rotation) => PluginState.SetSpotlightRotation(vehicle, rotation);

        public static Quaternion GetSpotlightRotation(Vehicle vehicle) => PluginState.GetSpotlightRotation(vehicle);
    }
}
