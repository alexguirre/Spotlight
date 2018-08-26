using System.Reflection;
using System.Runtime.InteropServices;
using Rage.Attributes;

[assembly: AssemblyTitle("Spotlight")]
[assembly: AssemblyDescription("")]
#if DEBUG
[assembly: AssemblyConfiguration("Debug")]
#else
[assembly: AssemblyConfiguration("Release")]
#endif
[assembly: AssemblyCompany("alexguirre")]
[assembly: AssemblyProduct("Spotlight")]
[assembly: AssemblyCopyright("Copyright ©  $CR_YEAR$ alexguirre")] // set by AppVeyor
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]
[assembly: ComVisible(false)]
[assembly: Guid("3ffad4ee-304c-47e6-8bdc-1dd20b9c6385")]
[assembly: AssemblyVersion("1.3.0.0")] // set by AppVeyor
[assembly: AssemblyFileVersion("1.3.0.0")] // set by AppVeyor
[assembly: AssemblyInformationalVersion("1.3")] // set by AppVeyor
[assembly: Plugin("Spotlight", Author = "alexguirre", PrefersSingleInstance = true, ShouldTickInPauseMenu = true)]