using Bashka.Plugins.Core.Abstract.Data;
using Bashka.Plugins.Core.FunctionBar;
using JetBrains.Annotations;

namespace Bashka.Plugins.Core.Orthodox;

[UsedImplicitly]
public class ShowLeftRootsCatalogCommand : IFunctionBarCatalogCommand
{
    protected bool LeftToActivate = true;
    protected bool RightToActivate = false;

    public bool CanExecute(CommandExecutionContext context) => true;

    public Task ExecuteAsync(CommandExecutionContext context)
    {
        OrthodoxCommander navigator = context.OrthodoxCommander!;
        navigator.LeftPanel.IsRootsVisible = LeftToActivate;
        navigator.RightPanel.IsRootsVisible = RightToActivate;
        return Task.CompletedTask;
    }
}