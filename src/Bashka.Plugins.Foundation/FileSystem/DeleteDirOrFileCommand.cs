using Bashka.Plugins.Core.Abstract;
using Bashka.Plugins.Core.Abstract.Data;
using Bashka.Plugins.Core.Abstract.Surrounding;
using Bashka.Plugins.Core.Dialog.Abstract;
using Bashka.Plugins.Core.Dialog.Basic;
using Bashka.Plugins.Core.FunctionBar;
using Bashka.Plugins.Foundation.ShellInteraction;
using Bashka.Plugins.Foundation.ShellInteraction.Scripting.Scripts;

namespace Bashka.Plugins.Foundation.FileSystem;

public class DeleteDirOrFileCommand : CatalogCommand<ShellCatalog, FileEntry>, IFunctionBarCatalogCommand
{
    private readonly IDialogProvider _dialogProvider;

    public DeleteDirOrFileCommand(IDialogProvider dialogProvider) : base(null, true, true)
    {
        _dialogProvider = dialogProvider;
    }

    protected override bool SupportsMultipleEntries => true;

    public override async Task ExecuteAsync(CatalogCommandContext<ShellCatalog, FileEntry> context)
    {
        IReadOnlyList<FileEntry> entries = context.Entries;

        string what = entries.Count == 1 ? entries[0].Name : $"{entries.Count} items";
        var okCancelDialogContent = new OkCancelQuestionDialogContent("Confirm delete",
            $"Are you sure you want to delete {what}?");

        bool result = await _dialogProvider.ShowDialogAsync<bool>(okCancelDialogContent);
        if(!result)
            return;

        foreach (FileEntry entry in entries)
        {
            ((ShellCatalog)entry.ParentCatalog)
                .ShellSession
                .ExecuteNonQuery(
                    new DeleteFileOrDirShellMultiScript(entry.IsDirectory, entry.FullPath));
            entry.MarkDeleted(); // todo: MarkDeleted seems too much concept. Better replace this entry with special DeletedEntry
        }
    }
}