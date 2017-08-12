namespace Spotlight.Core.Memory
{
    using System;
    using System.Runtime.InteropServices;

    using Rage;

    internal static unsafe class GameFunctions
    {
        public delegate long DrawCoronaDelegate(CCoronaDrawQueue* drawQueue, NativeVector3* position, float coronaSize, int colorARGB, float coronaIntensity, float coronaZBias, NativeVector3* direction, float unk_1, float innerAngle, float outerAngle, ushort unk_3);
        public delegate CLightDrawData* GetFreeLightDrawDataSlotFromPoolDelegate();
        public delegate CLightDrawData* InitializeLightDrawDataDelegate(CLightDrawData* data, eLightType type, uint flags, NativeVector3* position, NativeColorRGBAFloat* color, float intensity, int unk);
        public delegate void SetLightDrawDataDirectionDelegate(CLightDrawData* data, NativeVector3* direction, NativeVector3* unkNormalizedVec);
        public delegate void SetLightDrawDataAnglesDelegate(CLightDrawData* data, float innerAngle, float outerAngle);
        public delegate uint GetValueForLightDrawDataShadowUnkValueDelegate(CLightDrawData* data);

        public static DrawCoronaDelegate DrawCorona { get; private set; }
        public static GetFreeLightDrawDataSlotFromPoolDelegate GetFreeLightDrawDataSlotFromPool { get; private set; }
        public static InitializeLightDrawDataDelegate InitializeLightDrawData { get; private set; }
        public static SetLightDrawDataDirectionDelegate SetLightDrawDataDirection { get; private set; }
        public static SetLightDrawDataAnglesDelegate SetLightDrawDataAngles { get; private set; }
        public static GetValueForLightDrawDataShadowUnkValueDelegate GetValueForLightDrawDataShadowUnkValue { get; private set; }

        public static bool Init()
        {
            IntPtr address = Game.FindPattern("44 89 4C 24 ?? 48 83 EC 28 0F 29 74 24 ?? 0F 57 C0 0F 29 3C 24 4C 8B D2 4C 8B C9 0F 28 FA 0F 2E F8 0F 84 ?? ?? ?? ?? F3 0F 10 74 24 ?? 0F 2F F0");
            if (AssertAddress(address, nameof(DrawCorona)))
            {
                DrawCorona = Marshal.GetDelegateForFunctionPointer<DrawCoronaDelegate>(address);
            }

            address = Game.FindPattern("E8 ?? ?? ?? ?? 48 8B C8 48 8B F8 E8 ?? ?? ?? ?? 48 8D 45 A0 C7 44 24 ?? ?? ?? ?? ??");
            if (AssertAddress(address, nameof(GetFreeLightDrawDataSlotFromPool)))
            {
                address = address + *(int*)(address + 1) + 5;
                GetFreeLightDrawDataSlotFromPool = Marshal.GetDelegateForFunctionPointer<GetFreeLightDrawDataSlotFromPoolDelegate>(address);
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

            return !anyAssertFailed;
        }

        private static bool anyAssertFailed = false;
        private static bool AssertAddress(IntPtr address, string name)
        {
            if(address == IntPtr.Zero)
            {
                Game.LogTrivial($"Incompatible game version, couldn't find {name} function address.");
                anyAssertFailed = true;
                return false;
            }

            return true;
        }
    }
}
