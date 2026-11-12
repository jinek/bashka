using Bashka.Plugins.Core.Abstract;
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

    public CopyStreamCommand(IDialogProvider dialogProvider, WorkerTaskFactory workerTaskFactory) : base(null, true, true)
    {
        _dialogProvider = dialogProvider;
        _workerTaskFactory = workerTaskFactory;
    }

    public override bool CanExecute(CommandExecutionContext context)
    {
        return context.AnotherCatalog is IWriteStreamCatalog && //todo: better move this check to base type
               base.CanExecute(context);
    }

    public override async Task ExecuteAsync(CatalogCommandContext<IReadStreamCatalog, FileEntry> contextOuter)
    {
        CommandExecutionContext context = contextOuter.CommandExecutionContext;
        FileEntry entry = contextOuter.SingleEntry;

        string fileName =
            await _dialogProvider.ShowOkCancelDialogAsync(new StringInputDialog("Enter file name",
                entry.Name));

        await _workerTaskFactory.StartNewWorkerTask(async (cts, progress, _) =>
        {
            string destinationPath = Path.Combine(context.AnotherCatalog!.CurrentPath!, fileName);
            await using Stream sourceStream =
                ((IReadStreamCatalog)context.Catalog!).ReadStream(entry.FullPath);
            ((IWriteStreamCatalog)context.AnotherCatalog).WriteStream(destinationPath, sourceStream);
        });
        
        await context.AnotherCatalog!.NavigateAsync(string.Empty);
        
        Entry? newEntry = context.AnotherCatalog.Children.SingleOrDefault(entry => entry.Path == fileName);
        if (newEntry == null)
        {
            throw new UserErrorException("File was removed");
        }

        context.AnotherPanel!.SelectedEntry = newEntry;
    }
}