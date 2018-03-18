namespace Spotlight
{
    using System;

    using Spotlight.Core;

    [Serializable]
    public sealed class VehicleData
    {
        public const float DefaultOffsetX = -0.8f, DefaultOffsetY = 1.17f, DefaultOffsetZ = 0.52f;

        public XYZ Offset { get; set; }
        public bool DisableTurret { get; set; }
        public bool DisableTurretSpecified { get => DisableTurret == true; }

        public VehicleData()
        {
        }

        public VehicleData(XYZ offset, bool disableTurret = false)
        {
            Offset = offset;
            DisableTurret = disableTurret;
        }

        public static VehicleData Default => new VehicleData(new XYZ(DefaultOffsetX, DefaultOffsetY, DefaultOffsetZ));
    }
}
