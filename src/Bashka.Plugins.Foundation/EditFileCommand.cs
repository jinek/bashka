using Bashka.Plugins.Core.Abstract.Data;
using Bashka.Plugins.Core.Abstract.Surrounding;
using Bashka.Plugins.Core.FunctionKeys;
using Bashka.Plugins.Foundation.FileSystem;
using Bashka.Plugins.Foundation.ShellInteraction;

namespace Bashka.Plugins.Foundation;

public class EditFileCommand()
    : CatalogCommand<IPtyExecutor, FileEntry>(TextFileSupport.SupportedExtensions, false, true),
        IFunctionBarCatalogCommand
{
    public override async Task ExecuteAsync(CatalogCommandContext<IPtyExecutor, FileEntry> context)
    {
        var ptyExecutorCatalog = (IPtyExecutor)context.CommandExecutionContext.Catalog!;
        string editor = EditorLocator.FindEditor()
                        ?? throw new UserErrorException(
                            "No text editor was found. Install edit.net, micro, msedit, or nano and add it to PATH.");
        await ptyExecutorCatalog.ExecuteAsResidentAsync(
            await ptyExecutorCatalog.ResolveCurrentPathAsync(),
            editor, context.SingleEntry.FullPath, CancellationToken.None);
    }
}