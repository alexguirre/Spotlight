[assembly: Rage.Attributes.Plugin("Spotlight API Example", Author = "alexguirre", PrefersSingleInstance = true)]
namespace SpotlightAPIExample
{
    using System;
    using System.IO;
    using System.Reflection;

    internal static class EntryPoint
    {
        public static void Main()
        {
            AppDomain.CurrentDomain.AssemblyResolve += OnAssemblyResolve;
            Plugin.Run();
        }

        private static Assembly OnAssemblyResolve(object sender, ResolveEventArgs args)
        {
            if (args.Name.StartsWith("Spotlight"))
            {
                return Assembly.Load(File.ReadAllBytes(@"Plugins\Spotlight.dll"));
            }

            return null;
        }

        private static void OnUnload(bool isTerminating)
        {
            Plugin.End(isTerminating);
        }
    }
}
