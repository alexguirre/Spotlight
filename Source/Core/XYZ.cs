namespace Spotlight.Core
{
    using System;
    using System.Drawing;
    using System.Xml;
    using System.Xml.Schema;
    using System.Xml.Serialization;
    using System.Globalization;

    using Rage;

    public struct XYZ : IXmlSerializable
    {
        private float x, y, z;

        public float X => x;
        public float Y => y;
        public float Z => z;

        public XYZ(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }


        XmlSchema IXmlSerializable.GetSchema() => null;

        void IXmlSerializable.ReadXml(XmlReader reader)
        {
            reader.MoveToElement();

            x = Single.Parse(reader.GetAttribute("X"), CultureInfo.InvariantCulture);
            y = Single.Parse(reader.GetAttribute("Y"), CultureInfo.InvariantCulture);
            z = Single.Parse(reader.GetAttribute("Z"), CultureInfo.InvariantCulture);
        }

        void IXmlSerializable.WriteXml(XmlWriter writer)
        {
            writer.WriteAttributeString("X", X.ToString(CultureInfo.InvariantCulture));
            writer.WriteAttributeString("Y", Y.ToString(CultureInfo.InvariantCulture));
            writer.WriteAttributeString("Z", Z.ToString(CultureInfo.InvariantCulture));
        }

        public static implicit operator XYZ(Vector3 vector)
        {
            return new XYZ(vector.X, vector.Y, vector.Z);
        }

        public static implicit operator Vector3(XYZ xyz)
        {
            return new Vector3(xyz.x, xyz.y, xyz.z);
        }
    }
}
