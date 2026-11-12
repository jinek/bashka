using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using PropertyChanged;

namespace Bashka.Plugins.Core.Orthodox.UI.Helpers;

[DoNotNotify]
public class VerticalScrollDataGrid : DataGrid
{
    internal ScrollBar? ScrollBar;

    protected override Type StyleKeyOverride => typeof(DataGrid);

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        ScrollBar = e.NameScope.Find<ScrollBar>("PART_VerticalScrollbar");
    }
}