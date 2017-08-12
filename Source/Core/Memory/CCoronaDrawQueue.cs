namespace Spotlight.Core.Memory
{
    using System;
    using System.Runtime.InteropServices;

    using Rage;

    [StructLayout(LayoutKind.Explicit)]
    internal unsafe struct CCoronaDrawQueue
    {
        [FieldOffset(0x0000)] private CCoronaDrawCall queueStart;

        [FieldOffset(0xB400)] public int CallsCount;

        public CCoronaDrawCall* GetDrawCall(int index)
        {
            fixed(CCoronaDrawCall* queue = &queueStart)
            {
                return &queue[index];
            }
        }
    }
    [StructLayout(LayoutKind.Explicit, Size = 48)]
    internal unsafe struct CCoronaDrawCall
    {
        [FieldOffset(0x0000)] public NativeVector3 position;
        [FieldOffset(0x000C)] public uint field_C;
        [FieldOffset(0x0010)] public NativeVector3 field_10;
        [FieldOffset(0x0020)] public float size;
        [FieldOffset(0x0024)] public uint color;
        [FieldOffset(0x0028)] public float intensity;
        [FieldOffset(0x002C)] public float field_2C;
    }
}
