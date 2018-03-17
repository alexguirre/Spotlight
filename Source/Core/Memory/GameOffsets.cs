namespace Spotlight.Core.Memory
{
    using Rage;

    // offsets that are likely to change between versions
    internal static class GameOffsets
    {
        public static readonly int CVehicle_WeaponMgr;

        static GameOffsets()
        {
            switch (Game.ProductVersion.Build)
            {

                default:
                case 1290:
                    {
                        CVehicle_WeaponMgr = 0xBB0;
                    }
                    break;
            }
        }
    }
}
