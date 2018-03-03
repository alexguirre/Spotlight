namespace Spotlight
{
    using System;

    using Spotlight.Core;

    [Serializable]
    public sealed class VehicleData
    {
        public XYZ Offset { get; set; }
        public bool EnableTurret { get; set; }

        public VehicleData()
        {
        }

        public VehicleData(XYZ offset, bool enableTurret = false)
        {
            Offset = offset;
            EnableTurret = enableTurret;
        }

        public static VehicleData Default => new VehicleData(new XYZ(-0.8f, 1.17f, 0.52f));
    }
}
