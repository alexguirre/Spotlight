namespace Spotlight.Core.Memory
{
    using System;
    using System.Runtime.InteropServices;

    using Rage;

    internal enum eBoneRefId : int
    {
        extralight_1 = 124,
        extralight_2 = 125,
        extralight_3 = 126,
        extralight_4 = 127,

        turret_1base = 297,
        turret_2base = 299,
        turret_3base = 301,
        turret_4base = 303,

        turret_1barrel = 298,
        turret_2barrel = 300,
        turret_3barrel = 302,
        turret_4barrel = 304,
    }

    [StructLayout(LayoutKind.Explicit)]
    internal unsafe struct CVehicle
    {
        [FieldOffset(0x0030)] public fragInst* inst;

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
                IntPtr makeName = modelInfo + GameOffsets.CVehicleModelInfo_VehicleMakeName;
                return makeName;
            }
        }

        public IntPtr GetGameName()
        {
            fixed (CVehicle* v = &this)
            {
                IntPtr modelInfo = *(IntPtr*)((IntPtr)v + 0x20);
                IntPtr gameName = modelInfo + GameOffsets.CVehicleModelInfo_GameName;
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
    internal unsafe struct fragInst
    {
        [FieldOffset(0x68)] public fragCacheEntry* entry;
    }

    [StructLayout(LayoutKind.Explicit)]
    internal unsafe struct fragCacheEntry
    {
        [FieldOffset(0x148 + 0x30)] public crSkeleton* skeleton;
    }

    [StructLayout(LayoutKind.Explicit)]
    internal unsafe struct crSkeleton
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
        public void Draw(Vector3 position, float size, int color, float intensity, float zBias, Vector3 direction, float innerAngle, float outerAngle, ushort flags)
        {
            fixed (CCoronas* self = &this)
            {
                *tmpPos = position;
                *tmpDir = direction;
                GameFunctions.CCoronas_Draw(self, tmpPos, size, color, intensity, zBias, tmpDir, 1.0f, innerAngle, outerAngle, flags);
            }
        }

        /*
         * Allocate 16-byte aligned vectors since having Pack = 16 in NativeVector3's StructLayout does not ensure that it is 16-byte aligned
         * and CCoronos::Draw expects the vectors to be 16-byte aligned.
         * So copying the vector values here and passing these pointers to CCoronas::Draw should be good enough.
         * 
         * TODO: does C# have some way to force aligment of structs? Couldn't find it so far...
         */
        private static readonly NativeVector3* tmpPos = (NativeVector3*)(16 * (((long)Marshal.AllocHGlobal(sizeof(NativeVector3) + 8) + 15) / 16));
        private static readonly NativeVector3* tmpDir = (NativeVector3*)(16 * (((long)Marshal.AllocHGlobal(sizeof(NativeVector3) + 8) + 15) / 16));
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

        IgnoreArtificialLightsState = 0x10, // if set, keeps drawing the light after calling SET_ARTIFICIAL_LIGHTS_STATE(false)
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
