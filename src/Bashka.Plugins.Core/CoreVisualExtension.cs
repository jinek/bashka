using Bashka.Plugins.Core.Abstract;

namespace Bashka.Plugins.Core;

public class CoreVisualExtension : VisualExtension
{
    protected override IEnumerable<string> StyleUris =>
    [
        "avares://Bashka.Plugins.Core/Orthodox/UI/DarkTheme.axaml",
        "avares://Bashka.Plugins.Core/Worker/ProgressUI/ProgressDialogStyle.axaml"
    ];

    protected override IEnumerable<string> ResourceUris =>
    [
        "avares://Bashka.Plugins.Core/Orthodox/UI/Panel.axaml",
        "avares://Bashka.Plugins.Core/Orthodox/UI/VolkovCommander.axaml",
        "avares://Bashka.Plugins.Core/Dialog/Basic/OkCancelDialogContentBase.axaml",
        "avares://Bashka.Plugins.Core/Dialog/Basic/StringInputDialog.axaml",
        "avares://Bashka.Plugins.Core/Worker/ProgressUI/ProgressDialog.axaml"
    ];
}
