using Bashka.Plugins.Core.Abstract.Data;
using Bashka.Plugins.Core.Abstract.Surrounding;
using Bashka.Plugins.Core.Dialog.Abstract;
using Bashka.Plugins.Core.Dialog.Basic;
using Bashka.Plugins.Core.Worker.Schedulers;

namespace Bashka.Plugins.Market;

public sealed class DeletePluginPackageCommand(IDialogProvider dialogProvider, WorkerTaskFactory workerTaskFactory)
    : CatalogCommand<PluginsCatalog, PluginEntry>(null, false, true)
{
    protected override bool SupportsMultipleEntries => true;

    protected override bool CanExecuteForEntry(Entry entry)
    {
        return base.CanExecuteForEntry(entry) && ((PluginEntry)entry).InstalledVersion != null;
    }

    public override async Task ExecuteAsync(CatalogCommandContext<PluginsCatalog, PluginEntry> context)
    {
        IReadOnlyList<PluginEntry> entries = context.Entries;
        string what = entries.Count == 1 ? entries[0].Name : $"{entries.Count} plugins";
        bool confirmed = await dialogProvider.ShowDialogAsync(new OkCancelQuestionDialogContent("Confirm uninstall",
            $"Uninstall {what}? Restart Bashka to stop loaded plugins."));
        if (!confirmed) return;

        await workerTaskFactory.StartNewWorkerTask(async (cancellationToken, progress, uiTaskFactory) =>
            {
                try
                {
                    for (int index = 0; index < entries.Count; index++)
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        PluginEntry entry = entries[index];
                        progress.ReportProgress($"Uninstalling {entry.Name}...", (byte)(index * 100 / entries.Count));
                        System.IO.Directory.Delete(Path.Combine(MarketHelpers.PluginsDirectory, entry.Name), true);
                        await PluginMetadata.RefreshAsync(entry, uiTaskFactory);
                    }

                    progress.ReportProgress("Uninstallation complete", 100);
                }
                catch (Exception exception) when (exception is IOException or UnauthorizedAccessException
                                                      or OperationCanceledException)
                {
                    throw new UserErrorException(exception.Message);
                }
            }, $"Uninstalling {what}");
    }
}