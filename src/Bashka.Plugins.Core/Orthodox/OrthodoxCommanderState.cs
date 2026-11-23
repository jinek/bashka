using Bashka.Plugins.Core.Configuration;
using PropertyChanged;

namespace Bashka.Plugins.Core.Orthodox;

[SettingsSection("OrthodoxCommander(state)")]
[AddINotifyPropertyChangedInterface]
public class OrthodoxCommanderState
{
    /// <summary>
    ///     Last location of the default catalog in the left panel
    /// </summary>
    public string? LeftPath { get; set; }

    /// <summary>
    ///     <see cref="LeftPath" />
    /// </summary>
    public string? RightPath { get; set; }
}