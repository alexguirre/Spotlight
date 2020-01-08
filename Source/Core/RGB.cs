namespace Spotlight.Core
{
    using System;
    using System.Drawing;
    using System.Xml;
    using System.Xml.Schema;
    using System.Xml.Serialization;

    using Rage;

    public struct RGB : IXmlSerializable
    {
        private int raw;
        private byte r, g, b;

        public int Raw => raw;

        public byte R => r;
        public byte G => g;
        public byte B => b;

        public RGB(byte r, byte g, byte b)
        {
            raw = (255 << 24 | r << 16 | g << 8 | b << 0) & -1;

            this.r = r;
            this.g = g;
            this.b = b;
        }


        XmlSchema IXmlSerializable.GetSchema() => null;

        void IXmlSerializable.ReadXml(XmlReader reader)
        {
            reader.MoveToElement();

            byte r = (byte)MathHelper.Clamp(Int32.Parse(reader.GetAttribute("R")), 0, 255);
            byte g = (byte)MathHelper.Clamp(Int32.Parse(reader.GetAttribute("G")), 0, 255);
            byte b = (byte)MathHelper.Clamp(Int32.Parse(reader.GetAttribute("B")), 0, 255);

            this.r = r;
            this.g = g;
            this.b = b;

            raw = (255 << 24 | r << 16 | g << 8 | b << 0) & -1;
        }

        void IXmlSerializable.WriteXml(XmlWriter writer)
        {
            writer.WriteAttributeString("R", R.ToString());
            writer.WriteAttributeString("G", G.ToString());
            writer.WriteAttributeString("B", B.ToString());
        }

        public static implicit operator RGB(Color color)
        {
            return new RGB(color.R, color.G, color.B);
        }

        public static implicit operator Color(RGB color)
        {
            return Color.FromArgb(color.R, color.G, color.B);
        }
    }
}
