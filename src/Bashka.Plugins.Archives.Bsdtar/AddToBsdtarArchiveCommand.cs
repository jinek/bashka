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
/// Packs the selected entries into a new archive created next to them. Format is detected by bsdtar from the entered extension.
/// </summary>
public class AddToBsdtarArchiveCommand() : CatalogCommand<ShellCatalog, FileEntry>(null, true, true)
{
    private const string DefaultArchiveExtension = ".tar.gz";

    [AutoWire] public IDialogProvider DialogProvider { get; [UsedImplicitly] internal set; } = null!;

    [AutoWire] public WorkerTaskFactory WorkerTaskFactory { get; [UsedImplicitly] internal set; } = null!;

    protected override bool SupportsMultipleEntries => true;

    public override bool CanExecute(CommandExecutionContext context)
    {
        //packing entries of an archive being browsed is not supported
        return context.Catalog is not BsdtarArchiveCatalog && base.CanExecute(context);
    }

    public override async Task ExecuteAsync(CatalogCommandContext<ShellCatalog, FileEntry> context)
    {
        CommandExecutionContext innerContext = context.CommandExecutionContext;
        var catalog = (ShellCatalog)innerContext.Catalog!;
        IReadOnlyList<FileEntry> entries = context.Entries;

        string? archiveName = await DialogProvider.ShowOkCancelDialogAsync(
            new StringInputDialog("Enter archive name", entries[0].Name + DefaultArchiveExtension));
        if (archiveName == null)
            return;

        await WorkerTaskFactory.StartNewWorkerTask(async (cts, progress, _) =>
        {
            catalog.ShellSession.ExecuteNonQuery(new BsdtarCreateShellMultiScript(catalog.CurrentPath!,
                archiveName,
                BsdtarSupport.QuoteJoin(entries.Select(entry => entry.Name))));
        });

        foreach (FileEntry entry in entries)
            entry.IsMarked = false;

        await catalog.NavigateAsync(string.Empty);

        Entry? newEntry = catalog.Children.SingleOrDefault(child => child.Path == archiveName);
        if (newEntry != null)
            innerContext.Panel!.SelectedEntry = newEntry;
    }
}
