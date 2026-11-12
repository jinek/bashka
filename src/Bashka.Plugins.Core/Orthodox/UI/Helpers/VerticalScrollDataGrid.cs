using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using PropertyChanged;

namespace Bashka.Plugins.Core.Orthodox.UI.Helpers
{
    [DoNotNotify]
    public class VerticalScrollDataGrid : DataGrid
    {
        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            base.OnApplyTemplate(e);
            ScrollBar = e.NameScope.Find<ScrollBar>("PART_VerticalScrollbar");
        }

        protected override Type StyleKeyOverride => typeof(DataGrid);

        internal ScrollBar? ScrollBar;
    }
}