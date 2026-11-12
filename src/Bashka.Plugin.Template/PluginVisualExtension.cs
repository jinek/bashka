using Bashka.Plugins.Core.Abstract;

namespace Bashka.Plugin.Template;

public sealed class PluginVisualExtension : VisualExtension
{
    protected override IEnumerable<string> StyleUris =>
    [
        "avares://Bashka.Plugin.Template/Styles.axaml"
    ];
}
