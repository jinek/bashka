using Bashka.Plugins.Core.Abstract.Data;
using Bashka.Plugins.Core.Abstract.Surrounding;
using Bashka.Plugins.Core.Dialog.Abstract;
using Bashka.Plugins.Core.FunctionKeys;
using Bashka.Plugins.Core.Worker.Schedulers;

namespace Bashka.Plugins.Foundation.FileSystem;

public class StreamReadTextCatalogCommand(IDialogProvider dialogProvider, WorkerTaskFactory workerTaskFactory)
    : CatalogCommand<IReadStreamCatalog, Entry>(regexp: null, false, true), IFunctionBarCatalogCommand
{
    public const long MaxFileSize = 1024 * 1024;

    protected override bool CanExecuteForEntry(Entry entry)
    {
        return base.CanExecuteForEntry(entry) && entry.Size <= MaxFileSize;
    }

    public override async Task ExecuteAsync(CatalogCommandContext<IReadStreamCatalog, Entry> context)
    {
        Entry entry = context.SingleEntry;
        string fileContent = null!;
        await workerTaskFactory.StartNewWorkerTask((_, _, _) =>
        {
            using Stream stream =
                ((IReadStreamCatalog)context.CommandExecutionContext.Catalog!)
                .ReadStream(((FileEntry)entry).FullPath);
            using StreamReader streamReader = new(stream);
            fileContent = streamReader.ReadToEnd();
            return Task.CompletedTask;
        });
        await dialogProvider.ShowDialogAsync(
            new TextViewerDialogContent(entry.Name, fileContent));
    }
}