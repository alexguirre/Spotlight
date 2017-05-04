namespace Spotlight.Engine.Memory
{
    using System;
    using System.Diagnostics;
    using System.Runtime.InteropServices;

    internal static unsafe class GameFunctions
    {
        public delegate long DrawCoronaDelegate(long unkPtr, NativeVector3* position, float coronaSize, uint colorARGB, float coronaIntensity, float unk_100, NativeVector3* direction, float unk_1, float unk_0, float radius, ushort unk_3);

        private static DrawCoronaDelegate drawCorona;
        public static DrawCoronaDelegate DrawCorona
        {
            get
            {
                if(drawCorona == null)
                {
                    IntPtr baseAddress = Process.GetProcessesByName("GTA5")[0].MainModule.BaseAddress; // TODO: use pattern searching
                    drawCorona = Marshal.GetDelegateForFunctionPointer<DrawCoronaDelegate>(baseAddress + 0xD5DE94);
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
                    IntPtr baseAddress = Process.GetProcessesByName("GTA5")[0].MainModule.BaseAddress;
                    drawCoronaUnkPtr = (baseAddress + 0x25EBC90).ToInt64();
                }

                return drawCoronaUnkPtr;
            }
        }
    }
}
