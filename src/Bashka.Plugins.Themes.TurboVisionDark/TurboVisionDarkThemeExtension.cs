using Avalonia.Styling;
using Bashka.Plugins.Core.Abstract;
using Consolonia.Themes;

namespace Bashka.Plugins.Themes.TurboVisionDark;

public class TurboVisionDarkThemeExtension : VisualExtension
{
    protected override IEnumerable<string> StyleUris =>
        ["avares://Bashka.Plugins.Themes.TurboVisionDark/TurboVisionDarkTheme.axaml"];

    public override IEnumerable<IStyle> GetStyles()
    {
        return [new TurboVisionCompatibleTheme(), .. base.GetStyles()];
    }
}