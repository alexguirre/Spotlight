namespace Spotlight.Core.Memory
{
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal unsafe struct NativeColorRGBAFloat
    {
        public float R;
        public float G;
        public float B;
        public float A;
    }
}
