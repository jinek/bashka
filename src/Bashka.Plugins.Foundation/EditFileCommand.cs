using Bashka.Plugins.Core.Abstract.Data;
using Bashka.Plugins.Core.Abstract.Surrounding;
using Bashka.Plugins.Core.FunctionKeys;
using Bashka.Plugins.Foundation.FileSystem;
using Bashka.Plugins.Foundation.ShellInteraction;

namespace Bashka.Plugins.Foundation;

public class EditFileCommand(TextEditorSettings settings)
    : CatalogCommand<IPtyExecutor, FileEntry>(TextFileSupport.SupportedExtensions, false, true),
        IFunctionBarCatalogCommand
{
    public override async Task ExecuteAsync(CatalogCommandContext<IPtyExecutor, FileEntry> context)
    {
        var ptyExecutorCatalog = (IPtyExecutor)context.CommandExecutionContext.Catalog!;
        // todo: but serach for editor must happen thru shell too
        string editor = EditorLocator.FindEditor(settings.Editor)
                        ?? throw new UserErrorException(
                            settings.Editor.Equals(TextEditorSettings.AutoEditor, StringComparison.OrdinalIgnoreCase)
                                ? "No text editor was found. Install edit.net, micro, msedit, or nano and add it to PATH."
                                : $"Text editor '{settings.Editor}' was not found. Change Editor in [TextEditor] " +
                                  "section of a user settings file or set it to auto.");
        await ptyExecutorCatalog.ExecuteAsResidentAsync(
            await ptyExecutorCatalog.ResolveCurrentPathAsync(),
            editor, context.SingleEntry.FullPath, CancellationToken.None);
    }
}