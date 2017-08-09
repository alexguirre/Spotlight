namespace Spotlight.Core.Memory
{
    using System.Drawing;
    using System.Runtime.InteropServices;

    using Rage;

    [StructLayout(LayoutKind.Explicit, Size = 0x1C0)]
    internal unsafe struct CLightDrawData
    {
        [FieldOffset(0x0000)] public NativeVector3 Position;
        [FieldOffset(0x0010)] public NativeColorRGBAFloat Color;
        [FieldOffset(0x0020)] private NativeVector3 unk1;
        [FieldOffset(0x0030)] private NativeVector3 unk2;
        [FieldOffset(0x0040)] public NativeColorRGBAFloat VolumeOuterColor;
        [FieldOffset(0x0050)] private NativeVector3 unk3;
        [FieldOffset(0x0060)] public eLightType LightType;
        [FieldOffset(0x0064)] public uint Flags;
        [FieldOffset(0x0068)] public float Brightness;

        [FieldOffset(0x0070)] public int unkTxdDefPoolIndex;

        [FieldOffset(0x0078)] public float VolumeIntensity;
        [FieldOffset(0x007C)] public float VolumeSize;
        [FieldOffset(0x0080)] public float VolumeExponent;

        [FieldOffset(0x0098)] public float Range;
        [FieldOffset(0x009C)] public float FalloffExponent;

        public static CLightDrawData* New(eLightType type, eLightFlags flags, Vector3 position, Color color, float brightness)
        {
            CLightDrawData* d = GameFunctions.GetFreeLightDrawDataSlotFromPool();

            NativeVector3 pos = position;
            NativeColorRGBAFloat col = new NativeColorRGBAFloat { R = color.R / 255f, G = color.G / 255f, B = color.B / 255f, A = color.A / 255f };

            GameFunctions.CreateLightDrawData(d, eLightType.SPOT_LIGHT, (uint)flags, &pos, &col, brightness, 0xFFFFFF);

            return d;
        }
    }


    internal enum eLightType
    {
        RANGE = 1,
        SPOT_LIGHT = 2,
        RANGE_2 = 4,
    }

    internal enum eLightFlags : uint
    {
        VolumeConeVisible = 0x1000,

        VolumeOuterColorVisible = 0x80000,
    }
}
