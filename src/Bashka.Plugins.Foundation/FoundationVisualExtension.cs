using Bashka.Plugins.Core.Abstract;

namespace Bashka.Plugins.Foundation;

public class FoundationVisualExtension : VisualExtension
{
    protected override IEnumerable<string> StyleUris =>
    [
        "avares://Bashka.Plugins.Foundation/TestDialogStyles.axaml"
    ];

    protected override IEnumerable<string> ResourceUris =>
    [
        "avares://Bashka.Plugins.Foundation/TestDialog.axaml",
        "avares://Bashka.Plugins.Foundation/AboutDialogContent.axaml",
        "avares://Bashka.Plugins.Foundation/TextViewerDialogContent.axaml"
    ];
}