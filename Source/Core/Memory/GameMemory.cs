namespace Spotlight.Core.Memory
{
    using System;
    using System.Runtime.InteropServices;

    using Rage;

    internal static unsafe class GameMemory
    {
        public static CCoronaDrawQueue* CoronaDrawQueue { get; private set; }

        public static bool Init()
        {
            IntPtr address = Game.FindPattern("48 8D 0D ?? ?? ?? ?? F3 0F 11 44 24 ?? F3 0F 11 64 24 ?? E8 ?? ?? ?? ?? 4C 8D 9C 24 ?? ?? ?? ??");
            if (AssertAddress(address, nameof(CCoronaDrawQueue)))
            {
                address = address + *(int*)(address + 3) + 7;
                CoronaDrawQueue = (CCoronaDrawQueue*)address;
            }

            return !anyAssertFailed;
        }

        private static bool anyAssertFailed = false;
        private static bool AssertAddress(IntPtr address, string name)
        {
            if (address == IntPtr.Zero)
            {
                Game.LogTrivial($"Incompatible game version, couldn't find {name} instance.");
                anyAssertFailed = true;
                return false;
            }

            return true;
        }
    }
}
