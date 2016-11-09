namespace Spotlight
{
    // System
    using System;
    using System.Windows.Forms;

    internal static class Extensions
    {
        public static void ThreadSafeCall(this Control control, Action method)
        {
            if (method == null)
                throw new ArgumentNullException(nameof(method));

            if (control.InvokeRequired)
                control.Invoke(method);
            else
                method.Invoke(); ;
        }
    }
}
