using System.Reflection;

namespace Bashka.Plugins.Core.Configuration;

/// <summary>
///     Ini file embedded in a plugin, read-only default values
/// </summary>
public sealed record PluginSettingsFile(Assembly Assembly, string ResourceName, string Content)
{
    public string PluginName => Assembly.GetName().Name!;
}