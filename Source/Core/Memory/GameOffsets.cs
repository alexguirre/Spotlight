﻿namespace Spotlight.Core.Memory
{
    using System;

    using Rage;

    // offsets that are likely to change between versions
    internal static unsafe class GameOffsets
    {
        public static int CVehicle_WeaponMgr { get; private set; }
        public static int TlsAllocator { get; private set; }
        public static int CVehicleModelInfo_VehicleMakeName { get; private set; }
        public static int CVehicleModelInfo_GameName { get; private set; }

        public static bool Init()
        {
            IntPtr address = Game.FindPattern("48 8B 9B ?? ?? ?? ?? 48 85 DB 74 ?? 33 F6");
            if (AssertAddress(address, nameof(CVehicle_WeaponMgr)))
            {
                CVehicle_WeaponMgr = *(int*)(address + 3);
            }

            address = Game.FindPattern("B9 ?? ?? ?? ?? 48 8B 0C 01 45 33 C9 49 8B D2");
            if (AssertAddress(address, "TlsAllocatorOffset"))
            {
                TlsAllocator = *(int*)(address + 1);
            }

            address = Game.FindPattern("48 8D 82 ?? ?? ?? ?? 48 8D B2 ?? ?? ?? ?? 48 85 C0 74 09");
            if (AssertAddress(address, "CVehicleModelInfo_NamesOffsets"))
            {
                CVehicleModelInfo_VehicleMakeName = *(int*)(address + 3);
                CVehicleModelInfo_GameName = *(int*)(address + 10);
            }

            return !anyAssertFailed;
        }

        private static bool anyAssertFailed = false;
        private static bool AssertAddress(IntPtr address, string name)
        {
            if (address == IntPtr.Zero)
            {
                Game.LogTrivial($"Incompatible game version, couldn't find '{name}'.");
                anyAssertFailed = true;
                return false;
            }

            return true;
        }
    }
}
