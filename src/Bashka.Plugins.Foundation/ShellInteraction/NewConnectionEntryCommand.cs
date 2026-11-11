using Bashka.Plugins.Core.Abstract.Data;
using Bashka.Plugins.Core.Abstract.Surrounding;

namespace Bashka.Plugins.Foundation.ShellInteraction;

public class NewConnectionEntryCommand(NewConnectionCommand newConnectionCommand)
    : CatalogCommand<ConnectionsCatalog, NewConnectionEntry>(null, false, true)
{
    public override async Task ExecuteAsync(CatalogCommandContext<ConnectionsCatalog, NewConnectionEntry> context)
    {
        await newConnectionCommand.ExecuteAsync(context.CommandExecutionContext);
    }
}