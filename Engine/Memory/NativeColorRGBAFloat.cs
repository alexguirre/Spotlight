namespace Spotlight.Engine.Memory
{
    using System.Runtime.InteropServices;
    
    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct NativeColorRGBAFloat
    {
        public float R;
        public float G;
        public float B;
        public float A;
    }
}
