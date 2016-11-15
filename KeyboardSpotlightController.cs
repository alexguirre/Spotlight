namespace Spotlight
{
    // System
    using System.Windows.Forms;

    // RPH
    using Rage;

    internal class KeyboardSpotlightController : SpotlightController
    {
        private static bool keysSet;
        private static Keys moveLeftKey;
        private static Keys moveRightKey;
        private static Keys moveUpKey;
        private static Keys moveDownKey;


        public KeyboardSpotlightController(Spotlight owner) : base(owner)
        {
            if (!keysSet)
            {
                moveLeftKey = Plugin.Settings.GeneralSettingsIniFile.ReadEnum<Keys>("Keyboard", "Move Left", Keys.NumPad4);
                moveRightKey = Plugin.Settings.GeneralSettingsIniFile.ReadEnum<Keys>("Keyboard", "Move Right", Keys.NumPad6);
                moveUpKey = Plugin.Settings.GeneralSettingsIniFile.ReadEnum<Keys>("Keyboard", "Move Up", Keys.NumPad8);
                moveDownKey = Plugin.Settings.GeneralSettingsIniFile.ReadEnum<Keys>("Keyboard", "Move Down", Keys.NumPad2);

                keysSet = true;
            }

        }

        public override bool GetUpdatedRotationDelta(out Rotator rotation)
        {
            bool hasMoved = false;
            float pitch = 0.0f, yaw = 0.0f;

            if (Game.IsKeyDownRightNow(moveLeftKey))
            {
                hasMoved = true;
                yaw += Owner.Data.MovementSpeed;
            }
            else if (Game.IsKeyDownRightNow(moveRightKey))
            {
                hasMoved = true;
                yaw -= Owner.Data.MovementSpeed;
            }


            if (Game.IsKeyDownRightNow(moveUpKey))
            {
                hasMoved = true;
                pitch += Owner.Data.MovementSpeed;
            }
            else if (Game.IsKeyDownRightNow(moveDownKey))
            {
                hasMoved = true;
                pitch -= Owner.Data.MovementSpeed;
            }

            rotation = new Rotator(pitch, 0.0f, yaw);
            return hasMoved;
        }
    }
}
