using Avalonia.Styling;
using Bashka.Plugins.Core.Abstract;
using Consolonia.Themes;

namespace Bashka.Plugins.Core;

public class TurboVisionThemeExtension : VisualExtension
{
    protected override IEnumerable<string> StyleUris => ["avares://Bashka.Plugins.Core/Orthodox/UI/DarkTheme.axaml"];

    public override IEnumerable<IStyle> GetStyles()
    {
        return [new TurboVisionElegantTheme(), .. base.GetStyles()];
    }
}