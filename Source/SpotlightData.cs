namespace Spotlight
{
    using System;

    using Spotlight.Core;

    [Serializable]
    public sealed class SpotlightData
    {
        public RGB Color { get; set; }
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
        public bool Volume { get; set; }
        public bool Corona { get; set; }
        public bool Specular { get; set; }
        public float MovementSpeed { get; set; }

        public SpotlightData()
        {
        }

        public SpotlightData(RGB color, bool castShadows, float outerAngle, float innerAngle, float intensity, float range, float falloff, float volumeIntensity, float volumeSize, float coronaIntensity, float coronaSize, bool volume, bool corona, bool specular, float movementSpeed)
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
            Volume = volume;
            Corona = corona;
            Specular = specular;
            MovementSpeed = movementSpeed;
        }
    }
}
