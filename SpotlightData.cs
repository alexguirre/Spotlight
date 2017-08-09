namespace Spotlight
{
    // System
    using System;
    using System.Drawing;
    using System.Xml.Serialization;

    // RPH
    using Rage;

    [Serializable]
    public sealed class SpotlightData
    {
        [XmlElement(Type = typeof(Core.XmlColor))]
        public Color Color { get; set; }
        public bool Shadow { get; set; }
        public float Radius { get; set; }
        public float Brightness { get; set; }
        public float Distance { get; set; }
        public float Falloff { get; set; }
        public float Roundness { get; set; }
        public float MovementSpeed { get; set; }

        public SpotlightData() : this(Color.FromArgb(0), false, 0f, 0f, 0f, 0f, 0f, 0f)
        {
        }

        public SpotlightData(Color color, bool shadow, float radius, float brightness, float distance, float falloff, float roundness, float movementSpeed)
        {
            Color = color;
            Shadow = shadow;
            Radius = radius;
            Brightness = brightness;
            Distance = distance;
            Falloff = falloff;
            Roundness = roundness;
            MovementSpeed = movementSpeed;
        }
    }
}
