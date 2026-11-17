using System.Reflection;
using System.Runtime.Versioning;
using Bashka.Helpers;
using NuGet.Frameworks;

namespace Bashka.Plugins.Market;

public static class MarketHelpers
{
    public static readonly string PluginsDirectory = Path.Combine(BashkaHelpers.DataDirectory, "Plugins");
    public static readonly string PackagesDirectory = Path.Combine(BashkaHelpers.DataDirectory, "Packages");

    public static NuGetFramework GetFramework()
    {
        return NuGetFramework.ParseFrameworkName(
            typeof(InstallPluginCommand).Assembly.GetCustomAttribute<TargetFrameworkAttribute>()!.FrameworkName,
            DefaultFrameworkNameProvider.Instance);
    }
}