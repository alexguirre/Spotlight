namespace Spotlight.Core.Memory
{
    using System;
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
        [FieldOffset(0x0064)] public eLightFlags Flags;
        [FieldOffset(0x0068)] public float Intensity;

        [FieldOffset(0x0070)] public int unkTxdDefPoolIndex;

        [FieldOffset(0x0078)] public float VolumeIntensity;
        [FieldOffset(0x007C)] public float VolumeSize;
        [FieldOffset(0x0080)] public float VolumeExponent;

        [FieldOffset(0x0088)] public ulong ShadowRenderId;
        [FieldOffset(0x0090)] public uint ShadowUnkValue;
        [FieldOffset(0x0098)] public float Range;
        [FieldOffset(0x009C)] public float FalloffExponent;

        [FieldOffset(0x00D4)] public float ShadowNearClip; // default: 0.1

        public static CLightDrawData* New(eLightType type, eLightFlags flags, Vector3 position, Color color, float intensity)
        {
            const float ByteToFloatFactor = 1.0f / 255.0f;

            CLightDrawData* d = GameFunctions.GetFreeLightDrawDataSlotFromPool();

            NativeVector3 pos = position;
            NativeColorRGBAFloat col = new NativeColorRGBAFloat { R = color.R * ByteToFloatFactor, G = color.G * ByteToFloatFactor, B = color.B * ByteToFloatFactor, A = color.A * ByteToFloatFactor };

            GameFunctions.CreateLightDrawData(d, eLightType.SPOT_LIGHT, (uint)flags, &pos, &col, intensity, 0xFFFFFF);

            return d;
        }
    }


    internal enum eLightType
    {
        RANGE = 1,
        SPOT_LIGHT = 2,
        RANGE_2 = 4,
    }

    [Flags]
    internal enum eLightFlags : uint
    {
        None = 0,

        ShadowsFlag1 = 0x40, // both needed
        ShadowsFlag2 = 0x80,

        ShadowsFlag3 = 0x100, // needed, otherwise shadow flickers

        ShadowsEnabled = ShadowsFlag1 | ShadowsFlag2 | ShadowsFlag3,

        VolumeConeVisible = 0x1000,

        VolumeOuterColorVisible = 0x80000,
    }
}
