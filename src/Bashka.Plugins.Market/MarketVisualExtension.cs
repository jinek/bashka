using Bashka.Plugins.Core.Abstract;

namespace Bashka.Plugins.Market;

public sealed class MarketVisualExtension : VisualExtension
{
    protected override IEnumerable<string> ResourceUris =>
    [
        //"avares://Bashka.Plugins.Market/VersionSelectionDialog.axaml"
    ];
}
