namespace Spotlight.Core.Memory
{
    using System;
    using System.Threading;
    using System.Diagnostics;
    using System.Runtime.InteropServices;

    using Rage;

    using static Intrin;

    internal static unsafe class GameFunctions
    {
        public delegate long DrawCoronaDelegate(CCoronaDrawQueue* drawQueue, NativeVector3* position, float coronaSize, int colorARGB, float coronaIntensity, float coronaZBias, NativeVector3* direction, float unk_1, float innerAngle, float outerAngle, ushort unk_3);

        private static DrawCoronaDelegate drawCorona;
        public static DrawCoronaDelegate DrawCorona
        {
            get
            {
                if (drawCorona == null)
                {
                    IntPtr address = Game.FindPattern("44 89 4C 24 ?? 48 83 EC 28 0F 29 74 24 ?? 0F 57 C0 0F 29 3C 24 4C 8B D2 4C 8B C9 0F 28 FA 0F 2E F8 0F 84 ?? ?? ?? ?? F3 0F 10 74 24 ?? 0F 2F F0");
                    drawCorona = Marshal.GetDelegateForFunctionPointer<DrawCoronaDelegate>(address);
                }

                return drawCorona;
            }
        }


        public delegate CLightDrawData* GetFreeLightDrawDataSlotFromPoolDelegate();

        private static GetFreeLightDrawDataSlotFromPoolDelegate getFreeLightDrawDataSlotFromPool;
        public static GetFreeLightDrawDataSlotFromPoolDelegate GetFreeLightDrawDataSlotFromPool
        {
            get
            {
                if (getFreeLightDrawDataSlotFromPool == null)
                {
                    IntPtr address = Game.FindPattern("E8 ?? ?? ?? ?? 48 8B C8 48 8B F8 E8 ?? ?? ?? ?? 48 8D 45 A0 C7 44 24 ?? ?? ?? ?? ??");
                    address = address + *(int*)(address + 1) + 5;
                    getFreeLightDrawDataSlotFromPool = Marshal.GetDelegateForFunctionPointer<GetFreeLightDrawDataSlotFromPoolDelegate>(address);
                }

                return getFreeLightDrawDataSlotFromPool;
            }
        }


        public delegate CLightDrawData* CreateLightDrawDataDelegate(CLightDrawData* data, eLightType type, uint flags, NativeVector3* position, NativeColorRGBAFloat* color, float intensity, int unk);

        private static CreateLightDrawDataDelegate createLightDrawData;
        public static CreateLightDrawDataDelegate CreateLightDrawData
        {
            get
            {
                if (createLightDrawData == null)
                {
                    IntPtr address = Game.FindPattern("48 89 5C 24 ?? 48 89 6C 24 ?? 48 89 74 24 ?? 57 48 83 EC 40 83 A1 ?? ?? ?? ?? ?? 66 83 89 ?? ?? ?? ?? ??");
                    createLightDrawData = Marshal.GetDelegateForFunctionPointer<CreateLightDrawDataDelegate>(address);
                }

                return createLightDrawData;
            }
        }

        public delegate void SetLightDrawDataDirectionDelegate(CLightDrawData* data, NativeVector3* direction, NativeVector3* unkNormalizedVec);

        private static SetLightDrawDataDirectionDelegate setLightDrawDataDirection;
        public static SetLightDrawDataDirectionDelegate SetLightDrawDataDirection
        {
            get
            {
                if (setLightDrawDataDirection == null)
                {
                    IntPtr address = Game.FindPattern("F3 0F 10 22 F3 0F 10 42 ?? F3 0F 10 4A ?? 0F 57 D2 F3 0F 59 C0 F3 0F 59 C9 0F 28 DC F3 0F 10 2D ?? ?? ?? ?? F3 0F 59 DC F3 0F 58 D8 F3 0F 58 D9 0F 2E DA 75 05 0F 28 CA EB 0A 0F 51 C3");
                    setLightDrawDataDirection = Marshal.GetDelegateForFunctionPointer<SetLightDrawDataDirectionDelegate>(address);
                }

                return setLightDrawDataDirection;
            }
        }

        public delegate void SetLightDrawDataAnglesDelegate(CLightDrawData* data, float innerAngle, float outerAngle);

        private static SetLightDrawDataAnglesDelegate setLightDrawDataAngles;
        public static SetLightDrawDataAnglesDelegate SetLightDrawDataAngles
        {
            get
            {
                if (setLightDrawDataAngles == null)
                {
                    IntPtr address = Game.FindPattern("40 53 48 83 EC 40 0F 29 7C 24 ?? F3 0F 10 3D ?? ?? ?? ?? 48 8B D9 0F 2F D7 44 0F 29 44 24 ?? 73 06 44 0F 28 C7");
                    setLightDrawDataAngles = Marshal.GetDelegateForFunctionPointer<SetLightDrawDataAnglesDelegate>(address);
                }

                return setLightDrawDataAngles;
            }
        }

        public delegate bool DrawLightDelegate(CLightDrawData* data, long unk1, byte unk2);

        private static DrawLightDelegate drawLight;
        public static DrawLightDelegate DrawLight
        {
            get
            {
                if (drawLight == null)
                {
                    IntPtr address = Game.FindPattern("48 8B C4 48 89 58 08 48 89 70 10 48 89 78 18 4C 89 60 20 55 41 56 41 57 48 8D 68 A1 48 81 EC ?? ?? ?? ?? 0F 29 70 D8 45 33 E4 0F 29 78 C8");
                    drawLight = Marshal.GetDelegateForFunctionPointer<DrawLightDelegate>(address);
                }

                return drawLight;
            }
        }

        public delegate CLightDrawData* CopyLightDrawDataDelegate(CLightDrawData* destination, CLightDrawData* source);

        private static CopyLightDrawDataDelegate copyLightDrawData;
        public static CopyLightDrawDataDelegate CopyLightDrawData
        {
            get
            {
                if (copyLightDrawData == null)
                {
                    IntPtr address = Game.FindPattern("8B 02 F3 0F 10 4A ?? F3 0F 10 42 ?? 89 01 4C 8B D2 4C 8D 81 ?? ?? ?? ?? F3 0F 11 41 ?? F3 0F 11 49 ?? 8B 42 0C 41 B9 ?? ?? ?? ?? 89 41 0C 8B 42 10 F3 0F 10 4A ??");
                    copyLightDrawData = Marshal.GetDelegateForFunctionPointer<CopyLightDrawDataDelegate>(address);
                }

                return copyLightDrawData;
            }
        }

        public delegate uint GetValueForLightDrawDataShadowUnkValueDelegate(CLightDrawData* data);

        private static GetValueForLightDrawDataShadowUnkValueDelegate getValueForLightDrawDataShadowUnkValue;
        public static GetValueForLightDrawDataShadowUnkValueDelegate GetValueForLightDrawDataShadowUnkValue
        {
            get
            {
                if (getValueForLightDrawDataShadowUnkValue == null)
                {
                    IntPtr address = Game.FindPattern("4C 8B 81 ?? ?? ?? ?? 33 D2 48 8D 05 ?? ?? ?? ?? 4C 3B 00 74 18");
                    getValueForLightDrawDataShadowUnkValue = Marshal.GetDelegateForFunctionPointer<GetValueForLightDrawDataShadowUnkValueDelegate>(address);
                }

                return getValueForLightDrawDataShadowUnkValue;
            }
        }
    }
}
