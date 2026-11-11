using Bashka.Plugins.Core.Abstract.Data;
using Bashka.Plugins.Core.Abstract.Surrounding;
using Bashka.Plugins.Foundation.FileSystem;
using Bashka.Plugins.Foundation.ShellInteraction.Scripting.Scripts;

namespace Bashka.Plugins.Foundation.ShellInteraction;

public class ShellExecuteCommand : CatalogCommand<ShellCatalog, FileEntry>
{
    public ShellExecuteCommand() : base(null, false, true)
    {
    }

    public override Task ExecuteAsync(CatalogCommandContext<ShellCatalog, FileEntry> context)
    {
        FileEntry entry = context.SingleEntry;
        var catalog = (ShellCatalog)entry.ParentCatalog;
        catalog.ShellSession.ExecuteNonQuery(new ShellExecuteMultiScript(entry.Name));
        return Task.CompletedTask;
    }
}