namespace Spotlight.Engine
{
    // System
    using System;
    using System.Drawing;

    [Serializable]
    public sealed class XmlColor
    {
        public byte R { get; set; }
        public byte G { get; set; }
        public byte B { get; set; }

        public static implicit operator XmlColor(Color color)
        {
            return new XmlColor() { R = color.R, G = color.G, B = color.B };
        }

        public static implicit operator Color(XmlColor color)
        {
            return Color.FromArgb(color.R, color.G, color.B);
        }
    }
}
