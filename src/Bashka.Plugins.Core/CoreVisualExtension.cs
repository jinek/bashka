using Avalonia.Styling;
using Bashka.Plugins.Core.Abstract;
using Consolonia.Controls.DataGrid;
using Consolonia.Modal;

namespace Bashka.Plugins.Core;

public class CoreVisualExtension : VisualExtension
{
    protected override IEnumerable<string> StyleUris =>
    [
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

    public override IEnumerable<IStyle> GetStyles()
    {
        return [new ModalTheme(), new AutoDataGridStyles(), .. base.GetStyles()];
    }
}
