using System.Reflection;
using System.Runtime.InteropServices;
using Rage.Attributes;

[assembly: AssemblyTitle("Spotlight")]
[assembly: AssemblyDescription("")]
[assembly: AssemblyConfiguration(PluginInfo.Config)]
[assembly: AssemblyInformationalVersion(PluginInfo.Version + " - " + PluginInfo.Config)]
[assembly: AssemblyCompany("alexguirre")]
[assembly: AssemblyProduct("Spotlight")]
[assembly: AssemblyCopyright("Copyright ©  2015-2022 alexguirre")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]
[assembly: ComVisible(false)]
[assembly: Guid("3ffad4ee-304c-47e6-8bdc-1dd20b9c6385")]
[assembly: AssemblyVersion(PluginInfo.FullVersion)]
[assembly: AssemblyFileVersion(PluginInfo.FullVersion)]
[assembly: Plugin("Spotlight", Author = "alexguirre", PrefersSingleInstance = true, ShouldTickInPauseMenu = true)]

internal static class PluginInfo
{
#if DEBUG
    public const string Config = "Debug";
#else
    public const string Config = "Release";
#endif
    public const string Version = "1.3";
    public const string FullVersion = Version + ".1.0";
}
