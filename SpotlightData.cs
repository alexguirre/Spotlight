namespace Spotlight
{
    // System
    using System;
    using System.Drawing;
    using System.Xml.Serialization;

    // RPH
    using Rage;

    [Serializable]
    public struct SpotlightData
    {
        [XmlElement(Type = typeof(Engine.XmlColor))]
        public Color Color;
        public bool Shadow;
        public float Radius;
        public float Brightness;
        public float Distance;
        public float Falloff;
        public float Roundness;
        public float MovementSpeed;

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
