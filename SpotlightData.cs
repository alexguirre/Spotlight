namespace Spotlight
{
    using System;
    using System.Drawing;
    using System.Xml.Serialization;
    
    [Serializable]
    public sealed class SpotlightData
    {
        [XmlElement(Type = typeof(Core.XmlColor))]
        public Color Color { get; set; }
        public bool CastShadows { get; set; }
        public float OuterAngle { get; set; }
        public float InnerAngle { get; set; }
        public float Intensity { get; set; }
        public float Range { get; set; }
        public float Falloff { get; set; }
        public float VolumeIntensity { get; set; }
        public float VolumeSize { get; set; }
        public float CoronaIntensity { get; set; }
        public float CoronaSize { get; set; }
        public float MovementSpeed { get; set; }

        public SpotlightData()
        {
        }

        public SpotlightData(Color color, bool castShadows, float outerAngle, float innerAngle, float intensity, float range, float falloff, float volumeIntensity, float volumeSize, float coronaIntensity, float coronaSize, float movementSpeed)
        {
            Color = color;
            CastShadows = castShadows;
            OuterAngle = outerAngle;
            InnerAngle = innerAngle;
            Intensity = intensity;
            Range = range;
            Falloff = falloff;
            VolumeIntensity = volumeIntensity;
            VolumeSize = volumeSize;
            CoronaIntensity = coronaIntensity;
            CoronaSize = coronaSize;
            MovementSpeed = movementSpeed;
        }
    }
}
