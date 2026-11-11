using Bashka.Plugins.Core.Abstract.Data;
using Bashka.Plugins.Core.Abstract.Surrounding;
using Bashka.Plugins.Core.FunctionBar;
using Bashka.Plugins.Foundation.FileSystem;
using Bashka.Plugins.Foundation.ShellInteraction;

namespace Bashka.Plugins.Foundation
{
    public class NanoCatalogCommand()
        : CatalogCommand<IPtyExecutor, FileEntry>(TextFileSupport.SupportedExtensions, false, true),
            IFunctionBarCatalogCommand
    {
        public override async Task ExecuteAsync(CatalogCommandContext<IPtyExecutor, FileEntry> context)
        {
            var ptyExecutorCatalog = (IPtyExecutor)context.CommandExecutionContext.Catalog!;
            await NanoLauncher.OpenAsync(ptyExecutorCatalog,
                await ptyExecutorCatalog.ResolveCurrentPathAsync(),
                context.SingleEntry.Path);
        }
    }
}