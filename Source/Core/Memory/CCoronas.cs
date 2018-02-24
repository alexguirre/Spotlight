namespace Spotlight.Core.Memory
{
    using System;
    using System.Runtime.InteropServices;

    using Rage;

    [StructLayout(LayoutKind.Explicit)]
    internal unsafe struct CCoronas
    {
        [FieldOffset(0x0000)] private CCoronaDrawCall drawQueueStart;

        [FieldOffset(0xB400)] public int drawQueueCount;
        // plus fields for textures and settings from visualsettings.dat

        public CCoronaDrawCall* GetDrawCall(int index)
        {
            fixed (CCoronaDrawCall* queue = &drawQueueStart)
            {
                return &queue[index];
            }
        }

        public void Draw(Vector3 position, float size, int color, float intensity, float zBias, Vector3 direction, float innerAngle, float outerAngle, ushort a11)
        {
            if (size != 0.0 && intensity > 0.0 /*&& !g_IsBlackoutEnabled*/)
            {
                for (int index = drawQueueCount; index < 960; index = drawQueueCount)
                {
                    if (index == System.Threading.Interlocked.CompareExchange(ref drawQueueCount, index + 1, index))
                    {
                        float finalInnerAngle = innerAngle;
                        if (innerAngle >= 0.0f)
                        {
                            if (innerAngle > 90.0f)
                                finalInnerAngle = 90.0f;
                        }
                        else
                        {
                            finalInnerAngle = 0.0f;
                        }

                        float finalOuterAngle = 0.0f;
                        if (outerAngle >= 0.0f)
                        {
                            if (outerAngle <= 90.0f)
                                finalOuterAngle = outerAngle;
                            else
                                finalOuterAngle = 90.0f;
                        }

                        CCoronaDrawCall* call = GetDrawCall(index);
                        call->position[0] = position.X;
                        call->position[1] = position.Y;
                        call->position[2] = position.Z;
                        call->size = size;
                        call->color = color;
                        call->zBias = zBias;
                        call->intensity = intensity;
                        call->direction[0] = direction.X;
                        call->direction[1] = direction.Y;
                        call->direction[2] = direction.Z;
                        call->flags = a11 | (((ushort)finalOuterAngle | ((ushort)finalInnerAngle << 8)) << 16);
                        return;
                    }
                }
            }
        }
    }

    [StructLayout(LayoutKind.Sequential, Size = 48)]
    internal unsafe struct CCoronaDrawCall
    {
        public fixed float position[3];
        public int flags;
        public fixed float direction[3];
        public uint field_18;
        public float size;
        public int color;
        public float intensity;
        public float zBias;
    }
}
