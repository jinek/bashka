using System.Text;
using Bashka.Plugins.Core.Abstract.Data;
using Bashka.Plugins.Core.Abstract.Surrounding;
using Bashka.Plugins.Core.Dialog.Abstract;
using Bashka.Plugins.Core.FunctionKeys;
using Bashka.Plugins.Core.Worker.Schedulers;
using Bashka.Plugins.Foundation.FileSystem;
using Bashka.Plugins.Foundation.ShellInteraction;
using Bashka.Plugins.Foundation.ShellInteraction.Scripting.Scripts;

namespace Bashka.Plugins.Foundation;

public class FileCatCommand : CatalogCommand<ShellCatalog /*Could be ISttreamCatalog too*/, FileEntry>, IFunctionBarCatalogCommand
{
    public const long MaxFileSize = 1024 * 1024; // 10 MB

    private readonly IDialogProvider _dialogProvider;
    private readonly WorkerTaskFactory _workerTaskFactory;

    public FileCatCommand(IDialogProvider dialogProvider, WorkerTaskFactory workerTaskFactory) : base(regexp: null, false, true)
    {
        _dialogProvider = dialogProvider;
        _workerTaskFactory = workerTaskFactory;
    }

    protected override bool CanExecuteForEntry(Entry entry)
    {
        return base.CanExecuteForEntry(entry) && entry.Size <= MaxFileSize;
    }

    public override async Task ExecuteAsync(CatalogCommandContext<ShellCatalog, FileEntry> context)
    {
        FileEntry entry = context.SingleEntry;

        var result = new StringBuilder();
        await _workerTaskFactory.StartNewWorkerTask((_, _, _) =>
        {
            CatLine[] catLines =
            [
                .. ((ShellCatalog)entry
                    .ParentCatalog)
                .ShellSession
                .PrepareQuery(new CatShellMultiScript(entry.FullPath))
            ];

            foreach (string line in catLines.Select(x => x.Line))
            {
                result.Append(line);
                result.Append(Environment.NewLine);
            }
            if(result.Length>0)
                result.Remove(result.Length - 1, 1);
            return Task.CompletedTask;
            
        });
        
        await _dialogProvider.ShowDialogAsync(
            new TextViewerDialogContent(entry.Name, result.ToString()));
    }
}