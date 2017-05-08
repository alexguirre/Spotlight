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
        public static DrawCoronaDelegate DrawCorona // needs to be called in the frame render event
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
    }
}
