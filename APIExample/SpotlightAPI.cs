namespace SpotlightAPIExample
{
    using Spotlight.API;
    using System;
    using System.Reflection;
    using System.IO;
    using Rage;

    /// <summary>
    /// Wrapper class to avoid crash in case of missing Spotlight.dll.
    /// </summary>
    internal static class SpotlightAPI
    {
        private const string DllPath = @"Plugins\Spotlight.dll";
        private static readonly bool DllExists = File.Exists(DllPath);

        /// <summary>
        /// Gets whether the API functions can be called.
        /// The 'Spotlight.dll' must exist and has been loaded by the user.
        /// </summary>
        public static bool CanBeUsed => DllExists ? Functions.IsLoaded : false;

        public static bool HasSpotlight(this Vehicle vehicle) => Functions.HasSpotlight(vehicle);
        public static bool RequestSpotlight(this Vehicle vehicle) => Functions.RequestSpotlight(vehicle);
        public static void RequestSpotlightAndWait(this Vehicle vehicle) => Functions.RequestSpotlightAndWait(vehicle);
        public static void SetSpotlightTrackedEntity(this Vehicle vehicle, Entity entity) => Functions.SetSpotlightTrackedEntity(vehicle, entity);
        public static Entity GetSpotlightTrackedEntity(this Vehicle vehicle) => Functions.GetSpotlightTrackedEntity(vehicle);
        public static void SetSpotlightActive(this Vehicle vehicle, bool active) => Functions.SetSpotlightActive(vehicle, active);
        public static bool IsSpotlightActive(this Vehicle vehicle) => Functions.IsSpotlightActive(vehicle);
        public static void SetSpotlightInSearchMode(this Vehicle vehicle, bool searchModeActive) => Functions.SetSpotlightInSearchMode(vehicle, searchModeActive);
        public static bool IsSpotlightInSearchMode(this Vehicle vehicle) => Functions.IsSpotlightInSearchMode(vehicle);
        public static void SetSpotlightRotation(this Vehicle vehicle, Quaternion rotation) => Functions.SetSpotlightRotation(vehicle, rotation);
        public static Quaternion GetSpotlightRotation(this Vehicle vehicle) => Functions.GetSpotlightRotation(vehicle);

        static SpotlightAPI()
        {
            AppDomain.CurrentDomain.AssemblyResolve += OnAssemblyResolve;
        }

        private static Assembly OnAssemblyResolve(object sender, ResolveEventArgs args)
        {
            if (args.Name.StartsWith("Spotlight, ") && DllExists)
            {
                // load the Spotlight.dll located in the Plugins folder
                return Assembly.Load(File.ReadAllBytes(DllPath));
            }

            return null;
        }
    }
}
