namespace Spotlight.API
{
    public static class Functions
    {
        private static Settings userSettings;
        public static Settings GetUserSettings()
        {
            if (userSettings == null)
            {
                // TODO: fix GetUserSettings crash, cannot convert from Engine.XmlColor to System.Drawing.Color
                userSettings = new Settings(@"Plugins\Spotlight Resources\General.ini",
                                            @"Plugins\Spotlight Resources\Offsets.ini",
                                            @"Plugins\Spotlight Resources\Spotlight Data - Cars.xml",
                                            @"Plugins\Spotlight Resources\Spotlight Data - Helicopters.xml",
                                            @"Plugins\Spotlight Resources\Spotlight Data - Boats.xml",
                                            true);
            }

            return userSettings;
        }
    }
}
