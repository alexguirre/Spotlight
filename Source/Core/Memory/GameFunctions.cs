namespace Spotlight.Core.Memory
{
    using System;
    using System.Runtime.InteropServices;

    using Rage;

    internal static unsafe class GameFunctions
    {
        public delegate CLightDrawData* GetFreeLightDrawDataSlotFromQueueDelegate();
        public delegate CLightDrawData* InitializeLightDrawDataDelegate(CLightDrawData* data, eLightType type, uint flags, NativeVector3* position, NativeColorRGBAFloat* color, float intensity, int unk);
        public delegate void SetLightDrawDataDirectionDelegate(CLightDrawData* data, NativeVector3* direction, NativeVector3* dirPerpendicular);
        public delegate void SetLightDrawDataAnglesDelegate(CLightDrawData* data, float innerAngle, float outerAngle);
        public delegate uint GetValueForLightDrawDataShadowUnkValueDelegate(CLightDrawData* data);
        public delegate int GetLightEmissiveIndexForBoneDelegate(eBoneRefId bone);
        public delegate void CCoronas_DrawDelegate(CCoronas* self, NativeVector3* position, float size, int color, float intensity, float zBias, NativeVector3* direction, float a8, float innerAngle, float outerAngle, ushort flags);


        public static GetFreeLightDrawDataSlotFromQueueDelegate GetFreeLightDrawDataSlotFromQueue { get; private set; }
        public static InitializeLightDrawDataDelegate InitializeLightDrawData { get; private set; }
        public static SetLightDrawDataDirectionDelegate SetLightDrawDataDirection { get; private set; }
        public static SetLightDrawDataAnglesDelegate SetLightDrawDataAngles { get; private set; }
        public static GetValueForLightDrawDataShadowUnkValueDelegate GetValueForLightDrawDataShadowUnkValue { get; private set; }
        public static GetLightEmissiveIndexForBoneDelegate GetLightEmissiveIndexForBone { get; private set; }
        public static CCoronas_DrawDelegate CCoronas_Draw { get; private set; }

        public static bool Init()
        {
            IntPtr address = Game.FindPattern("80 ?? 3A 00 74 07 41 81 CE ?? ?? ?? ?? E8 ?? ?? ?? ??");
            if (AssertAddress(address, nameof(GetFreeLightDrawDataSlotFromQueue)))
            {
                address += 13;
                address = address + *(int*)(address + 1) + 5;
                GetFreeLightDrawDataSlotFromQueue = Marshal.GetDelegateForFunctionPointer<GetFreeLightDrawDataSlotFromQueueDelegate>(address);
            }

            address = Game.FindPattern("48 89 5C 24 ?? 48 89 6C 24 ?? 48 89 74 24 ?? 57 48 83 EC 40 83 A1 ?? ?? ?? ?? ?? 66 83 89 ?? ?? ?? ?? ??");
            if (AssertAddress(address, nameof(InitializeLightDrawData)))
            {
                InitializeLightDrawData = Marshal.GetDelegateForFunctionPointer<InitializeLightDrawDataDelegate>(address);
            }

            address = Game.FindPattern("F3 0F 10 22 F3 0F 10 42 ?? F3 0F 10 4A ?? 0F 57 D2 F3 0F 59 C0 F3 0F 59 C9 0F 28 DC F3 0F 10 2D ?? ?? ?? ?? F3 0F 59 DC");
            if (AssertAddress(address, nameof(SetLightDrawDataDirection)))
            {
                SetLightDrawDataDirection = Marshal.GetDelegateForFunctionPointer<SetLightDrawDataDirectionDelegate>(address);
            }

            address = Game.FindPattern("40 53 48 83 EC 40 0F 29 7C 24 ?? F3 0F 10 3D ?? ?? ?? ?? 48 8B D9 0F 2F D7 44 0F 29 44 24 ?? 73 06 44 0F 28 C7");
            if (AssertAddress(address, nameof(SetLightDrawDataAngles)))
            {
                SetLightDrawDataAngles = Marshal.GetDelegateForFunctionPointer<SetLightDrawDataAnglesDelegate>(address);
            }

            address = Game.FindPattern("4C 8B 81 ?? ?? ?? ?? 33 D2 48 8D 05 ?? ?? ?? ?? 4C 3B 00 74 18");
            if (AssertAddress(address, nameof(GetValueForLightDrawDataShadowUnkValue)))
            {
                GetValueForLightDrawDataShadowUnkValue = Marshal.GetDelegateForFunctionPointer<GetValueForLightDrawDataShadowUnkValueDelegate>(address);
            }

            address = Game.FindPattern("83 F9 ?? 7F 57 74 4F 83 E9 ??");
            if (AssertAddress(address, nameof(GetLightEmissiveIndexForBone)))
            {
                GetLightEmissiveIndexForBone = Marshal.GetDelegateForFunctionPointer<GetLightEmissiveIndexForBoneDelegate>(address);
            }

            address = Game.FindPattern("44 89 4C 24 ?? 48 83 EC 28 0F 29 74 24 ?? 0F 57 C0 ");
            if (AssertAddress(address, nameof(CCoronas_Draw)))
            {
                CCoronas_Draw = Marshal.GetDelegateForFunctionPointer<CCoronas_DrawDelegate>(address);
            }

            return !anyAssertFailed;
        }

        private static bool anyAssertFailed = false;
        private static bool AssertAddress(IntPtr address, string name)
        {
            if (address == IntPtr.Zero)
            {
                Game.LogTrivial($"Incompatible game version, couldn't find {name} function address.");
                anyAssertFailed = true;
                return false;
            }

            return true;
        }
    }
}
