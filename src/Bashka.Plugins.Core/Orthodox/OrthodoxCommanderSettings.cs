using Bashka.Plugins.Core.Configuration;

// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace Bashka.Plugins.Core.Orthodox;

[SettingsSection(nameof(OrthodoxCommander))]
public class OrthodoxCommanderSettings
{
    public bool AltCharActivatesSearch { get; init; }
}