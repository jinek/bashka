using Bashka.Plugins.Core.Abstract;
using Bashka.Plugins.Core.Abstract.Data;
using Bashka.Plugins.Core.Abstract.Surrounding;
using Bashka.Plugins.Core.Dialog.Abstract;
using Bashka.Plugins.Core.Dialog.Basic;
using Bashka.Plugins.Core.Scoping;
using Bashka.Plugins.Core.Worker.Schedulers;
using Bashka.Plugins.Foundation.ShellInteraction;
using Bashka.Plugins.Foundation.ShellInteraction.Scripting;
using JetBrains.Annotations;

namespace Bashka.Plugins.Foundation.FileSystem;

public abstract class CopyOrMoveCommandBase : CatalogCommand<ShellCatalog, FileEntry>
{
    protected CopyOrMoveCommandBase() : base(null, true, true)
    {
    }

    [AutoWire] public IDialogProvider DialogProvider { get; [UsedImplicitly] internal set; } = null!;

    [AutoWire] public WorkerTaskFactory WorkerTaskFactory { get; [UsedImplicitly] internal set; } = null!;

    /// <summary>
    /// When false the entry stays in the current catalog, i.e. it is just renamed
    /// </summary>
    protected virtual bool ToAnotherPanel => true;

    /// <summary>
    /// When true the source entry disappears from its catalog, so it is marked as moved
    /// </summary>
    protected virtual bool SourceIsGone => false;

    protected abstract string DialogTitle { get; }

    /// <summary>
    /// Renaming (staying in the same catalog) makes sense for a single entry only
    /// </summary>
    protected override bool SupportsMultipleEntries => ToAnotherPanel;

    protected abstract ShellMultiScript CreateScript(string sourcePath, string destinationPath);

    public override bool CanExecute(CommandExecutionContext context)
    {
        if (!base.CanExecute(context))
            return false;

        if (!ToAnotherPanel)
            return true;

        if (context.AnotherCatalog is not ShellCatalog shellCatalog)
            return false;

        return ((ShellCatalog)context.Catalog!)
               .ShellSession
               .ShellConnection
               .IsEquivalentTo(shellCatalog.ShellSession.ShellConnection);
    }

    public override async Task ExecuteAsync(CatalogCommandContext<ShellCatalog, FileEntry> context)
    {
        CommandExecutionContext innerContext = context.CommandExecutionContext;

        ICatalog destinationCatalog = (ToAnotherPanel ? innerContext.AnotherCatalog : innerContext.Catalog)!;
        IPanel destinationPanel = (ToAnotherPanel ? innerContext.AnotherPanel : innerContext.Panel)!;

        IReadOnlyList<FileEntry> entries = context.Entries;

        string? singleFileName = null;
        if (entries.Count == 1)
        {
            singleFileName =
                await DialogProvider.ShowOkCancelDialogAsync(new StringInputDialog(DialogTitle,
                    entries[0].Name));
        }
        else
        {
            bool confirmed = await DialogProvider.ShowDialogAsync(new OkCancelQuestionDialogContent("Confirm",
                $"{(SourceIsGone ? "Move" : "Copy")} {entries.Count} items to {destinationCatalog.CurrentPath}?"));
            if (!confirmed)
                return;
        }

        await WorkerTaskFactory.StartNewWorkerTask(async (_, _, _) =>
        {
            foreach (FileEntry entry in entries)
            {
                string sourcePath = Path.Combine(innerContext.Catalog!.CurrentPath!, entry.Name);
                string destinationPath = Path.Combine(destinationCatalog.CurrentPath!, singleFileName ?? entry.Name);
                ((ShellCatalog)innerContext.Catalog)
                    .ShellSession
                    .ExecuteNonQuery(CreateScript(sourcePath, destinationPath));
            }
        });

        foreach (FileEntry entry in entries)
        {
            if (SourceIsGone)
                entry.MarkMoved();
            entry.IsMarked = false;
        }

        await destinationCatalog.NavigateAsync(string.Empty);

        if (singleFileName == null)
            return;

        Entry? newEntry = destinationCatalog.Children.SingleOrDefault(child => child.Path == singleFileName);
        if (newEntry == null)
        {
            throw new UserErrorException("File was removed after this command");
        }
        
        destinationPanel.SelectedEntry = newEntry;
    }
}
