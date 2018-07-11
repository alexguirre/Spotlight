namespace Spotlight
{
    using System;

    using Spotlight.Core;

    [Serializable]
    public sealed class VehicleData
    {
        public const string IniKeyX = "X", IniKeyY = "Y", IniKeyZ = "Z", IniKeyDisableTurret = "DisableTurret";

        public const float DefaultOffsetX = -0.8f, DefaultOffsetY = 1.17f, DefaultOffsetZ = 0.52f;
        public const bool DefaultDisableTurret = false;


        public XYZ Offset { get; set; }
        public bool DisableTurret { get; set; }
        public bool DisableTurretSpecified { get => DisableTurret != DefaultDisableTurret; }

        public VehicleData()
        {
        }

        public VehicleData(XYZ offset, bool disableTurret = DefaultDisableTurret)
        {
            Offset = offset;
            DisableTurret = disableTurret;
        }

        public static VehicleData Default => new VehicleData(new XYZ(DefaultOffsetX, DefaultOffsetY, DefaultOffsetZ), DefaultDisableTurret);
    }
}
