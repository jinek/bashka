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
/// Extracts the selected archives into the directory of the other panel.
/// </summary>
public class ExtractBsdtarArchiveCommand()
    : CatalogCommand<ShellCatalog, FileEntry>(BsdtarSupport.SupportedExtensions, false, true)
{
    [AutoWire] public IDialogProvider DialogProvider { get; [UsedImplicitly] internal set; } = null!;

    [AutoWire] public WorkerTaskFactory WorkerTaskFactory { get; [UsedImplicitly] internal set; } = null!;

    protected override bool SupportsMultipleEntries => true;

    public override bool CanExecute(CommandExecutionContext context)
    {
        if (context.Catalog is BsdtarArchiveCatalog || !base.CanExecute(context))
            return false;

        if (context.AnotherCatalog is not ShellCatalog destinationCatalog ||
            destinationCatalog is BsdtarArchiveCatalog)
            return false;

        return ((ShellCatalog)context.Catalog!)
            .ShellSession
            .ShellConnection
            .IsEquivalentTo(destinationCatalog.ShellSession.ShellConnection);
    }

    public override async Task ExecuteAsync(CatalogCommandContext<ShellCatalog, FileEntry> context)
    {
        CommandExecutionContext innerContext = context.CommandExecutionContext;
        var catalog = (ShellCatalog)innerContext.Catalog!;
        ICatalog destinationCatalog = innerContext.AnotherCatalog!;
        IReadOnlyList<FileEntry> entries = context.Entries;

        bool confirmed = await DialogProvider.ShowDialogAsync<bool>(new OkCancelQuestionDialogContent("Confirm",
            $"Extract {entries.Count} archive(s) to {destinationCatalog.CurrentPath}?"));
        if (!confirmed)
            return;

        await WorkerTaskFactory.StartNewWorkerTask(async (cts, progress, _) =>
        {
            foreach (FileEntry entry in entries)
            {
                catalog.ShellSession.ExecuteNonQuery(new BsdtarExtractToDirectoryShellMultiScript(
                    Path.Combine(catalog.CurrentPath!, entry.Name),
                    destinationCatalog.CurrentPath!,
                    string.Empty));
            }
        });

        foreach (FileEntry entry in entries)
            entry.IsMarked = false;

        await destinationCatalog.NavigateAsync(string.Empty);
    }
}
