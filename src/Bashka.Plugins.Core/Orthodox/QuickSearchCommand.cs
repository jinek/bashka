using Bashka.Plugins.Core.Abstract;
using Bashka.Plugins.Core.Abstract.Data;
using JetBrains.Annotations;

namespace Bashka.Plugins.Core.Orthodox;

[UsedImplicitly]
public class QuickSearchCommand : ICommand
{
    public bool CanExecute(CommandExecutionContext context) =>
        context.Panel is { IsQuickSearchActive: false, CurrentCatalog: not null };

    public Task ExecuteAsync(CommandExecutionContext context)
    {
        context.Panel!.IsQuickSearchActive = true;
        return Task.CompletedTask;
    }
}
