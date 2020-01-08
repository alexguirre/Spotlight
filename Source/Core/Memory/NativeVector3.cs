namespace Spotlight.Core.Memory
{
    using System.Runtime.InteropServices;

    using Rage;

    [StructLayout(LayoutKind.Sequential, Size = 0x10, Pack = 0x10)]
    internal unsafe struct NativeVector3
    {
        public float X;
        public float Y;
        public float Z;
        private float padding;

        public static implicit operator Vector3(NativeVector3 value) => new Vector3(value.X, value.Y, value.Z);
        public static implicit operator NativeVector3(Vector3 value) => new NativeVector3 { X = value.X, Y = value.Y, Z = value.Z };
    }
}
