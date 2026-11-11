using System.Reactive;
using System.Reactive.Linq;
using Bashka.Plugins.Core.Abstract.Data;
using Bashka.Plugins.Core.FunctionBar;
using JetBrains.Annotations;

namespace Bashka.Plugins.Core.Orthodox;

[UsedImplicitly]
public class NavigateToMenuCommand : IFunctionBarCatalogCommand
{
    public bool CanExecute(CommandExecutionContext context) => true;

    public async Task ExecuteAsync(CommandExecutionContext context)
    {
        await context.OrthodoxCommander!.FocusMenuInteraction.Handle(Unit.Default);
    }
}
