using Bashka.Helpers;
using Bashka.Plugins.Core.Abstract.Data;

namespace Bashka.Plugins.Core.Abstract.Surrounding;

public class GoBackCommand : CatalogCommand<ICatalog, GoBackEntry>
{
    public GoBackCommand() : base(string.Empty, true, true)
    {
    }

    public override async Task ExecuteAsync(CatalogCommandContext<ICatalog, GoBackEntry> context)
    {
        IPanel panel = context.CommandExecutionContext.Panel!;
        ICatalog catalog = context.CommandExecutionContext.Catalog!;
        panel.SelectedIndex = -1;

        if (catalog.CanNavigateBack)
        {
            string lastDirName = BashkaHelpers.PathSegments(catalog.CurrentPath!)[^1];

            await catalog.NavigateBack();

            Entry[] currentChildren = [.. panel.CurrentChildren!];
            Entry lastEntry = currentChildren.Single(entry1 => entry1.Path == lastDirName);
            int index = currentChildren.IndexOf(lastEntry);
            panel.SelectedIndex = index;
        }
        else
        {
            panel.Pop(); //todo: remember position
        }
    }
}