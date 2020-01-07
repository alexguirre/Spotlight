namespace Spotlight.Core.Memory
{
    using System;

    using Rage;

    // offsets that are likely to change between versions
    internal static class GameOffsets
    {
        public static readonly int CVehicle_WeaponMgr;

        unsafe static GameOffsets()
        {
            IntPtr addr = Game.FindPattern("48 8B 9B ?? ?? ?? ?? 48 85 DB 74 ?? 33 F6");

            CVehicle_WeaponMgr = *(int*)(addr + 3);
        }
    }
}
