namespace Spotlight
{
    using System;

    using Spotlight.Core;

    [Serializable]
    public sealed class VehicleData
    {
        public const string IniKeyX = "X", IniKeyY = "Y", IniKeyZ = "Z", IniKeyDisableTurret = "DisableTurret", IniKeySpotlightExtraLight = "SpotlightExtraLight";

        public const float DefaultOffsetX = -0.8f, DefaultOffsetY = 1.17f, DefaultOffsetZ = 0.52f;
        public const bool DefaultDisableTurret = false;
        public const int DefaultSpotlightExtraLight = 0;


        public XYZ Offset { get; set; }
        public bool DisableTurret { get; set; }
        public bool DisableTurretSpecified { get => DisableTurret != DefaultDisableTurret; }
        public int SpotlightExtraLight { get; set; }
        public bool SpotlightExtraLightSpecified { get => SpotlightExtraLight > DefaultSpotlightExtraLight && SpotlightExtraLight <= 4; }

        public VehicleData()
        {
        }

        public VehicleData(XYZ offset, bool disableTurret = DefaultDisableTurret, int spotlightExtraLight = DefaultSpotlightExtraLight)
        {
            Offset = offset;
            DisableTurret = disableTurret;
            SpotlightExtraLight = spotlightExtraLight;
        }

        public static VehicleData Default => new VehicleData(new XYZ(DefaultOffsetX, DefaultOffsetY, DefaultOffsetZ));
    }
}
