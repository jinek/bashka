using Bashka.Plugins.Core.Configuration;

// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace Bashka.Plugins.Core.Orthodox;

[SettingsSection(nameof(OrthodoxCommander))]
public class OrthodoxCommanderSettings
{
    public bool AltCharActivatesSearch { get; init; }

    /// <summary>
    ///     Location of the left panel when started by a launcher, wins over <see cref="OrthodoxCommanderState.LeftPath" />
    /// </summary>
    public string? StartupLocationLeft { get; init; }

    /// <summary>
    ///     <see cref="StartupLocationLeft" />
    /// </summary>
    public string? StartupLocationRight { get; init; }
}