namespace Spotlight
{
    // System
    using System;
    using System.IO;
    using System.Threading;
    using System.Windows.Forms;

    // RPH
    using Rage;

    internal static class Plugin
    {
        public static Settings Settings { get; private set; }

        private static void Main()
        {
            while (Game.IsLoading)
                GameFiber.Sleep(500);

            if (!Directory.Exists(@"Plugins\Spotlight Resources\"))
                Directory.CreateDirectory(@"Plugins\Spotlight Resources\");

            Settings = new Settings(@"Plugins\Spotlight Resources\general.ini",
                                    @"Plugins\Spotlight Resources\offsets.ini",
                                    @"Plugins\Spotlight Resources\cars spotlight data.xml",
                                    @"Plugins\Spotlight Resources\helicopters spotlight data.xml",
                                    @"Plugins\Spotlight Resources\boats spotlight data.xml",
                                    true);

            while (true)
            {
                GameFiber.Yield();

                Update();
            }
        }

        private static void Update()
        {
        }

        private static void OnUnload(bool isTerminating)
        {
            if (!isTerminating)
            {
                // native calls: delete entities, blips, etc.

            }

            // dispose objects
        }
    }
}

/* RUN WINDOWS FORM
EditSettingsForm = new EditSettingsForm();
FormsThread = new Thread(() =>
{
    Application.EnableVisualStyles();
    Application.Run(EditSettingsForm);
});
FormsThread.SetApartmentState(ApartmentState.STA);
FormsThread.Start();
*/
