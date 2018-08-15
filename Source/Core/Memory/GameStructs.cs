namespace Spotlight.Core.Memory
{
    using System;
    using System.Runtime.InteropServices;

    using Rage;

    internal enum eBoneRefId : int
    {
        extralight_1 = 108,
        extralight_2 = 109,
        extralight_3 = 110,
        extralight_4 = 111,

        turret_1base = 281,
        turret_2base = 283,
        turret_3base = 285,
        turret_4base = 287,
        
        turret_1barrel = 282,
        turret_2barrel = 284,
        turret_3barrel = 286,
        turret_4barrel = 288,
    }

    [StructLayout(LayoutKind.Explicit)]
    internal unsafe struct CVehicle
    {
        [FieldOffset(0x0030)] public fragInstGta* inst;

        [FieldOffset(0x0BB0)] public CVehicleWeaponMgr* weaponMgr;

        public CVehicleWeaponMgr* GetWeaponMgr()
        {
            fixed (CVehicle* v = &this)
            {
                return *(CVehicleWeaponMgr**)((IntPtr)v + GameOffsets.CVehicle_WeaponMgr);
            }
        }

        public byte* GetBoneRefsArray()
        {
            fixed (CVehicle* v = &this)
            {
                //vehicle->modelInfo->unkStructB0->struct->boneRefs;
                IntPtr modelInfo = *(IntPtr*)((IntPtr)v + 0x20);
                IntPtr unkStruct = *(IntPtr*)(modelInfo + 0xB0);
                IntPtr vehicleStruct = *(IntPtr*)(unkStruct);
                IntPtr boneRefs = (vehicleStruct + 0x10);
                return (byte*)boneRefs;
            }
        }

        public byte GetBoneIndex(eBoneRefId boneRefId)
        {
            return GetBoneRefsArray()[(int)boneRefId];
        }

        public IntPtr GetMakeName()
        {
            fixed (CVehicle* v = &this)
            {
                IntPtr modelInfo = *(IntPtr*)((IntPtr)v + 0x20);
                IntPtr makeName = modelInfo + GameMemory.CVehicleModelInfoVehicleMakeNameOffset;
                return makeName;
            }
        }

        public IntPtr GetGameName()
        {
            fixed (CVehicle* v = &this)
            {
                IntPtr modelInfo = *(IntPtr*)((IntPtr)v + 0x20);
                IntPtr gameName = modelInfo + GameMemory.CVehicleModelInfoGameNameOffset;
                return gameName;
            }
        }

        public void SetLightEmissive(int index, float value)
        {
            fixed (CVehicle* v = &this)
            {
                IntPtr drawHandler = *(IntPtr*)((IntPtr)v + 0x48);
                if (drawHandler == IntPtr.Zero)
                {
                    return;
                }

                IntPtr customShaderEffect = *(IntPtr*)(drawHandler + 0x20);
                if (customShaderEffect == IntPtr.Zero)
                {
                    return;
                }

                float* lightEmissives = (float*)(customShaderEffect + 0x20);
                lightEmissives[index] = value;
            }
        }
    }

    [StructLayout(LayoutKind.Explicit)]
    internal unsafe struct CVehicleWeaponMgr
    {
        [FieldOffset(0x110)] public int TurretCount;
        [FieldOffset(0x114)] public int WeaponCount;

        public int GetMaxTurrets() { return 6; }
        public int GetMaxWeapons() { return 6; }

        public CTurret* GetTurret(int index)
        {
            fixed (CVehicleWeaponMgr* v = &this)
            {
                IntPtr* turrets = (IntPtr*)((IntPtr)v + 0x8); 
                return (CTurret*)(turrets[index]);
            }
        }

        public CVehicleWeapon* GetWeapon(int index)
        {
            fixed (CVehicleWeaponMgr* v = &this)
            {
                IntPtr* weapons = (IntPtr*)((IntPtr)v + 0x68);
                return (CVehicleWeapon*)(weapons[index]);
            }
        }
    }

    [StructLayout(LayoutKind.Explicit)]
    internal unsafe struct CTurret
    {
        [FieldOffset(0x0010)] public eBoneRefId baseBoneRefId;
        [FieldOffset(0x0014)] public eBoneRefId barrelBoneRefId;

        [FieldOffset(0x20)] public Quaternion rot1;
        [FieldOffset(0x30)] public Quaternion rot2;
        [FieldOffset(0x40)] public Quaternion rot3;
    }

    [StructLayout(LayoutKind.Explicit)]
    internal unsafe struct CVehicleWeapon
    {
        [FieldOffset(0x0020)] public eBoneRefId weaponBoneRefId;

        public uint GetName()
        {
            //vehWeapon->weapon->info->nameHash;
            fixed (CVehicleWeapon* v = &this)
            {
                IntPtr weapon = *(IntPtr*)((IntPtr)v + 0x18);
                if (weapon != IntPtr.Zero)
                {
                    IntPtr info = *(IntPtr*)(weapon + 0x40);
                    if (info != IntPtr.Zero)
                    {
                        uint name = *(uint*)(info + 0x10);
                        return name;
                    }
                }
            }
            return 0xFFFFFFFF;
        }
    }

    [StructLayout(LayoutKind.Explicit)]
    internal unsafe struct fragInstGta
    {
        [FieldOffset(0x0010)] public phArchetypeDamp* archetype;
    }

    [StructLayout(LayoutKind.Explicit)]
    internal unsafe struct phArchetypeDamp
    {
        [FieldOffset(0x0158)] public CEntitySkeleton* skeleton;
    }

    [StructLayout(LayoutKind.Explicit)]
    internal unsafe struct CEntitySkeleton
    {
        [FieldOffset(0x0000)] public crSkeletonData* skeletonData;
        [FieldOffset(0x0008)] public Matrix* entityTransform;
        [FieldOffset(0x0010)] public Matrix* desiredBonesTransformsArray;
        [FieldOffset(0x0018)] public Matrix* currentBonesTransformsArray;
        [FieldOffset(0x0020)] public int bonesCount;
    }

    [StructLayout(LayoutKind.Explicit)]
    internal unsafe struct crSkeletonData
    {
        [FieldOffset(0x0020)] public crSkeletonBoneData* bones;
        [FieldOffset(0x0028)] public Matrix* bonesTransformationsInverted;
        [FieldOffset(0x0030)] public Matrix* bonesTransformations;
        [FieldOffset(0x0038)] public ushort* bonesParentIndices;
        [FieldOffset(0x005E)] public ushort bonesCount;

        public string GetBoneNameForIndex(uint index)
        {
            if (index >= bonesCount)
                return null;

            return bones[index].GetName();
        }
    }

    [StructLayout(LayoutKind.Explicit, Size = 0x50)]
    internal unsafe struct crSkeletonBoneData
    {
        [FieldOffset(0x0000)] public Quaternion rotation;
        [FieldOffset(0x0010)] public NativeVector3 translation;

        [FieldOffset(0x0032)] public ushort parentIndex;

        [FieldOffset(0x0038)] public IntPtr namePtr;

        [FieldOffset(0x0042)] public ushort index;

        public string GetName() => namePtr == null ? null : Marshal.PtrToStringAnsi(namePtr);
    }

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

        [FieldOffset(0x0070)] public int TextureDictIndex;
        [FieldOffset(0x0074)] public uint TextureNameHash;
        [FieldOffset(0x0078)] public float VolumeIntensity;
        [FieldOffset(0x007C)] public float VolumeSize;
        [FieldOffset(0x0080)] public float VolumeExponent;

        [FieldOffset(0x0088)] public ulong ShadowRenderId;
        [FieldOffset(0x0090)] public uint ShadowUnkValue;
        [FieldOffset(0x0098)] public float Range;
        [FieldOffset(0x009C)] public float FalloffExponent;

        [FieldOffset(0x00D4)] public float ShadowNearClip; // default: 0.1

        public static CLightDrawData* New(eLightType type, eLightFlags flags, Vector3 position, RGB color, float intensity)
        {
            const float ByteToFloatFactor = 1.0f / 255.0f;

            CLightDrawData* d = GameFunctions.GetFreeLightDrawDataSlotFromQueue();

            NativeVector3 pos = position;
            NativeColorRGBAFloat col = new NativeColorRGBAFloat { R = color.R * ByteToFloatFactor, G = color.G * ByteToFloatFactor, B = color.B * ByteToFloatFactor };

            GameFunctions.InitializeLightDrawData(d, type, (uint)flags, &pos, &col, intensity, 0xFFFFFF);

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

        CanRenderUnderground = 0x8, // if not set the light won't render in underground parts of the map, such as tunnels

        HasTexture = 0x20,

        ShadowsFlag1 = 0x40, // needed
        ShadowsFlag2 = 0x80, // needed
        ShadowsFlag3 = 0x100, // needed, otherwise shadow flickers
        ShadowsFlag4 = 0x4000000, // needed, otherwise the shadow doesn't render properly sometimes

        EnableShadows = ShadowsFlag1 | ShadowsFlag2 | ShadowsFlag3 | ShadowsFlag4,


        EnableVolume = 0x1000,
        UseVolumeOuterColor = 0x80000,


        DisableSpecular = 0x2000,

        IgnoreGlass = 0x800000, // if set the light won't affect glass

        DisableLight = 0x40000000, // if set the light isn't rendered, can be used to draw only the volume
    }
}
