echo ^
using System.Reflection; ^
[assembly: AssemblyTitle("KSFrameowork (http://github.com/mr-kelly/KSFramework)")]^
[assembly: AssemblyDescription("KSFramework - KEngine + XLua")]^
[assembly: AssemblyConfiguration("")]^
[assembly: AssemblyCompany("Kelly")]^
[assembly: AssemblyProduct("KSFramework")]^
[assembly: AssemblyCopyright("Copyright @ Kelly<23110388@qq.com> 2015-2016")]^
[assembly: AssemblyTrademark("")]^
[assembly: AssemblyCulture("")]^
[assembly: AssemblyVersion("%appveyor_build_version%")]^
namespace KSFramework^
{^
    public class KSFrameworkInfo^
    {^
        public static readonly string Version = "%appveyor_build_version%";^
    }^
} > KSFramework/Assets/KSFrameworkInfo.cs

