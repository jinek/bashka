using Bashka.Plugins.Core.Abstract;
using Bashka.Plugins.Core.Abstract.Data;
using Bashka.Plugins.Core.Abstract.Surrounding;

namespace Bashka.Plugins.Core.Orthodox
{
    public class SwitchConnectionCommand : CatalogCommand<ICatalog, SwitchConnectionEntry>
    {
        public SwitchConnectionCommand() : base(null, false, true)
        {
        }

        public override async Task ExecuteAsync(CatalogCommandContext<ICatalog, SwitchConnectionEntry> context)
        {
            IPanel panel = context.CommandExecutionContext.Panel!;
            SwitchConnectionEntry entry = context.SingleEntry;
            IShellConnection
                shellConnection = entry.ConnectionFactory(); //todo: disposal of the connection when switched away


            //todo: how to properly dispose the scope without disposing underlying objects?
            OrthodoxCommander orthodoxCommander = context.CommandExecutionContext.OrthodoxCommander!;

            IPanel newPanel = orthodoxCommander.RecreatePanel(panel, shellConnection);
            newPanel.SelectedIndex = -1;
            await Task.Yield();//todo: seems some avalonia bug??
            newPanel.IsFocused = true;
            newPanel.SelectedIndex = 0;
        }
    }
}