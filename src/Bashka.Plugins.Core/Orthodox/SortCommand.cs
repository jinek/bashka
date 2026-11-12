using Bashka.Plugins.Core.Abstract;
using Bashka.Plugins.Core.Abstract.Data;
using Bashka.Plugins.Core.FunctionKeys;
using Bashka.Plugins.Core.Scoping;

namespace Bashka.Plugins.Core.Orthodox;

public class SortCommand(SortKey sortKey) : TypeKeyedObject, IFunctionBarCommand<ICatalog>
{
    public bool CanExecute(CommandExecutionContext context)
    {
        return context.Panel != null;
    }

    public Task ExecuteAsync(CommandExecutionContext context)
    {
        IPanel panel = context.Panel!;
        panel.SortMode = panel.SortMode.Toggle(sortKey);
        return Task.CompletedTask;
    }
}