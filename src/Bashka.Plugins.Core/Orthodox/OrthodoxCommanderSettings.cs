using Bashka.Plugins.Core.Configuration;
using PropertyChanged;

// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace Bashka.Plugins.Core.Orthodox;

[SettingsSection(nameof(OrthodoxCommander))]
[AddINotifyPropertyChangedInterface]
public class OrthodoxCommanderSettings
{
    public bool AltCharActivatesSearch { get; init; }

    /// <summary>
    ///     State: location of the default catalog in the left panel
    /// </summary>
    public string? LeftPath { get; set; }

    /// <summary>
    ///     <see cref="LeftPath" />
    /// </summary>
    public string? RightPath { get; set; }
}