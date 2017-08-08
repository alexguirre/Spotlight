namespace Spotlight.Engine.Memory
{
    using System;
    using System.Diagnostics;
    using System.Runtime.InteropServices;

    using Rage;

    internal static unsafe class GameFunctions
    {
        public delegate long DrawCoronaDelegate(long unkPtr, NativeVector3* position, float coronaSize, uint colorARGB, float coronaIntensity, float unk_100, NativeVector3* direction, float unk_1, float innerAngle, float outerAngle, ushort unk_3);

        private static DrawCoronaDelegate drawCorona;
        public static DrawCoronaDelegate DrawCorona
        {
            get
            {
                if(drawCorona == null)
                {
                    IntPtr address = Game.FindPattern("44 89 4C 24 ?? 48 83 EC 28 0F 29 74 24 ?? 0F 57 C0 0F 29 3C 24 4C 8B D2 4C 8B C9 0F 28 FA 0F 2E F8 0F 84 ?? ?? ?? ?? F3 0F 10 74 24 ?? 0F 2F F0");
                    drawCorona = Marshal.GetDelegateForFunctionPointer<DrawCoronaDelegate>(address);
                }

                return drawCorona;
            }
        }

        private static long drawCoronaUnkPtr;
        public static long DrawCoronaUnkPtr
        {
            get
            {
                if(drawCoronaUnkPtr == 0)
                {
                    IntPtr address = Game.FindPattern("48 8D 0D ?? ?? ?? ?? F3 0F 11 44 24 ?? F3 0F 11 64 24 ?? E8 ?? ?? ?? ?? 4C 8D 9C 24 ?? ?? ?? ?? 49 8B 5B 28 49 8B 73 30 41 0F 28 73 ?? 41 0F 28 7B ?? 45 0F 28 43 ?? 45 0F 28 4B ??");
                    address = address + *(int*)(address + 3) + 7;
                    drawCoronaUnkPtr = (long)address;
                }

                return drawCoronaUnkPtr;
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
                    getFreeLightDrawDataSlotFromPool = Marshal.GetDelegateForFunctionPointer<GetFreeLightDrawDataSlotFromPoolDelegate>(Process.GetCurrentProcess().MainModule.BaseAddress + 0x4E1C28);
                }

                return getFreeLightDrawDataSlotFromPool;
            }
        }


        public delegate CLightDrawData* CreateLightDrawDataDelegate(CLightDrawData* data, eLightType type, uint flags, NativeVector3* position, NativeColorRGBAFloat* color, float brightness, int unk);

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

        public delegate void SetLightDrawDataRoundnessAndRadiusDelegate(CLightDrawData* data, float roundness, float radius);

        private static SetLightDrawDataRoundnessAndRadiusDelegate setLightDrawDataRoundnessAndRadiusDelegate;
        public static SetLightDrawDataRoundnessAndRadiusDelegate SetLightDrawDataRoundnessAndRadius
        {
            get
            {
                if (setLightDrawDataRoundnessAndRadiusDelegate == null)
                {
                    setLightDrawDataRoundnessAndRadiusDelegate = Marshal.GetDelegateForFunctionPointer<SetLightDrawDataRoundnessAndRadiusDelegate>(Process.GetCurrentProcess().MainModule.BaseAddress + 0x228E0);
                }

                return setLightDrawDataRoundnessAndRadiusDelegate;
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


        //
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
    }
}
