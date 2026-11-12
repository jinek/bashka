using Bashka.Plugins.Core.Abstract.Data;
using Bashka.Plugins.Core.Abstract.Surrounding;
using Bashka.Plugins.Core.Dialog.Abstract;
using Bashka.Plugins.Core.Dialog.Basic;
using Bashka.Plugins.Core.Worker.Schedulers;

namespace Bashka.Plugins.Foundation.FileSystem;

public class CopyStreamCommand : CatalogCommand<IReadStreamCatalog, FileEntry>
{
    private readonly IDialogProvider _dialogProvider;
    private readonly WorkerTaskFactory _workerTaskFactory;

    public CopyStreamCommand(IDialogProvider dialogProvider, WorkerTaskFactory workerTaskFactory) : base(null, true,
        true)
    {
        _dialogProvider = dialogProvider;
        _workerTaskFactory = workerTaskFactory;
    }

    public override bool CanExecute(CommandExecutionContext context)
    {
        return context.AnotherCatalog is IWriteStreamCatalog && //todo: better move this check to base type
               base.CanExecute(context);
    }

    public override async Task ExecuteAsync(CatalogCommandContext<IReadStreamCatalog, FileEntry> context)
    {
        CommandExecutionContext innerContext = context.CommandExecutionContext;
        FileEntry entry = context.SingleEntry;

        string fileName =
            await _dialogProvider.ShowOkCancelDialogAsync(new StringInputDialog("Enter file name",
                entry.Name));

        await _workerTaskFactory.StartNewWorkerTask(async (_, _, _) =>
        {
            string destinationPath = Path.Combine(innerContext.AnotherCatalog!.CurrentPath!, fileName);
            await using Stream sourceStream =
                ((IReadStreamCatalog)innerContext.Catalog!).ReadStream(entry.FullPath);
            ((IWriteStreamCatalog)innerContext.AnotherCatalog).WriteStream(destinationPath, sourceStream);
        });

        await innerContext.AnotherCatalog!.NavigateAsync(string.Empty);

        Entry? newEntry = innerContext.AnotherCatalog.Children.SingleOrDefault(entry2 => entry2.Path == fileName);
        if (newEntry == null) throw new UserErrorException("File was removed");

        innerContext.AnotherPanel!.SelectedEntry = newEntry;
    }
}