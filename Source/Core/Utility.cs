namespace Spotlight.Core
{
    using System;
    using System.Drawing;
    using System.Diagnostics;
    using System.Windows.Forms;
    using System.Runtime.InteropServices;

    using Rage;
    using Rage.Native;

    internal static class Utility
    {
        static Utility()
        {
            // executing it here first prevents a small freeze during gameplay the first time GetDisabledControlNormal is used, for example when using the MouseSpotlightInputController for the first time
            GetDisabledControlNormal((GameControl)0);
        }

        public static bool IsKeyDownWithModifier(Keys key, Keys modifier)
        {
            return modifier == Keys.None ? Game.IsKeyDown(key) : (Game.IsKeyDownRightNow(modifier) && Game.IsKeyDown(key));
        }

        public static bool IsKeyDownRightNowWithModifier(Keys key, Keys modifier)
        {
            return modifier == Keys.None ? Game.IsKeyDownRightNow(key) : (Game.IsKeyDownRightNow(modifier) && Game.IsKeyDownRightNow(key));
        }

        public static bool IsControllerButtonDownWithModifier(ControllerButtons button, ControllerButtons modifier)
        {
            return modifier == ControllerButtons.None ? Game.IsControllerButtonDown(button) : (Game.IsControllerButtonDownRightNow(modifier) && Game.IsControllerButtonDown(button));
        }

        public static bool IsControllerButtonDownRightNowWithModifier(ControllerButtons button, ControllerButtons modifier)
        {
            return modifier == ControllerButtons.None ? Game.IsControllerButtonDownRightNow(button) : (Game.IsControllerButtonDownRightNow(modifier) && Game.IsControllerButtonDownRightNow(button));
        }

        public static float GetDisabledControlNormal(GameControl control)
        {
            return NativeFunction.Natives.GetDisabledControlNormal<float>(0, (int)control);
        }
    }
}
