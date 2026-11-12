using Bashka.Plugins.Archives.Bsdtar.Scripts;
using Bashka.Plugins.Core.Abstract;
using Bashka.Plugins.Core.Abstract.Data;
using Bashka.Plugins.Core.Abstract.Surrounding;
using Bashka.Plugins.Core.Dialog.Abstract;
using Bashka.Plugins.Core.Dialog.Basic;
using Bashka.Plugins.Core.Scoping;
using Bashka.Plugins.Core.Worker.Schedulers;
using JetBrains.Annotations;
using Bashka.Plugins.Foundation.FileSystem;
using Bashka.Plugins.Foundation.ShellInteraction;

namespace Bashka.Plugins.Archives.Bsdtar;

/// <summary>
/// Extracts the selected entries of a browsed archive into the directory of the other panel.
/// </summary>
public class ExtractBsdtarEntriesCommand() : CatalogCommand<BsdtarArchiveCatalog, FileEntry>(null, true, true)
{
    [AutoWire] public IDialogProvider DialogProvider { get; [UsedImplicitly] internal set; } = null!;

    [AutoWire] public WorkerTaskFactory WorkerTaskFactory { get; [UsedImplicitly] internal set; } = null!;

    protected override bool SupportsMultipleEntries => true;

    public override bool CanExecute(CommandExecutionContext context)
    {
        if (!base.CanExecute(context))
            return false;

        if (context.AnotherCatalog is not ShellCatalog destinationCatalog ||
            destinationCatalog is BsdtarArchiveCatalog)
            return false;

        return ((BsdtarArchiveCatalog)context.Catalog!)
            .ShellSession
            .ShellConnection
            .IsEquivalentTo(destinationCatalog.ShellSession.ShellConnection);
    }

    public override async Task ExecuteAsync(CatalogCommandContext<BsdtarArchiveCatalog, FileEntry> context)
    {
        CommandExecutionContext innerContext = context.CommandExecutionContext;
        var catalog = (BsdtarArchiveCatalog)innerContext.Catalog!;
        ICatalog destinationCatalog = innerContext.AnotherCatalog!;
        IReadOnlyList<FileEntry> entries = context.Entries;

        bool confirmed = await DialogProvider.ShowDialogAsync<bool>(new OkCancelQuestionDialogContent("Confirm",
            $"Extract {entries.Count} item(s) to {destinationCatalog.CurrentPath}?"));
        if (!confirmed)
            return;

        //a directory entry matches only itself, so an extra glob brings its content too
        string patterns = BsdtarSupport.QuoteJoin(entries.SelectMany(entry => entry.IsDirectory
            ? new[] { entry.FullPath, entry.FullPath + "/*" }
            : [entry.FullPath]));

        await WorkerTaskFactory.StartNewWorkerTask(async (cts, progress, _) =>
        {
            catalog.ShellSession.ExecuteNonQuery(new BsdtarExtractToDirectoryShellMultiScript(
                catalog.ArchiveEntry.FullPath,
                destinationCatalog.CurrentPath!,
                patterns));
        });

        foreach (FileEntry entry in entries)
            entry.IsMarked = false;

        await destinationCatalog.NavigateAsync(string.Empty);
    }
}
