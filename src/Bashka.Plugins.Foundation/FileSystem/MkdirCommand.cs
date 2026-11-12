using Bashka.Plugins.Core.Abstract.Data;
using Bashka.Plugins.Core.Abstract.Surrounding;
using Bashka.Plugins.Core.Dialog.Abstract;
using Bashka.Plugins.Core.Dialog.Basic;
using Bashka.Plugins.Core.FunctionKeys;
using Bashka.Plugins.Core.Scoping;
using Bashka.Plugins.Core.Worker.Schedulers;
using Bashka.Plugins.Foundation.ShellInteraction;
using Bashka.Plugins.Foundation.ShellInteraction.Scripting.Scripts;

namespace Bashka.Plugins.Foundation.FileSystem;

public class MkdirCommand : TypeKeyedObject, IFunctionBarCommand<ShellCatalog>
{
    private readonly IDialogProvider _dialogProvider;
    private readonly WorkerTaskFactory _workerTaskFactory;

    public MkdirCommand(IDialogProvider dialogProvider, WorkerTaskFactory workerTaskFactory)
    {
        _dialogProvider = dialogProvider;
        _workerTaskFactory = workerTaskFactory;
    }

    public bool CanExecute(CommandExecutionContext context)
    {
        return true;
    }

    public async Task ExecuteAsync(CommandExecutionContext context)
    {
        context.Panel!.SelectedIndex = -1;
        
        string fileName = await _dialogProvider.ShowOkCancelDialogAsync(new StringInputDialog("Enter directory name"));

        await _workerTaskFactory.StartNewWorkerTask(async (_, _, _) =>
        {
            string path = Path.Combine(context.Catalog!.CurrentPath!, fileName);
            ((ShellCatalog)context.Catalog)
                .ShellSession
                .ExecuteNonQuery(new MkdirMultiScript(path));
        });

        await context.Catalog!.NavigateAsync(string.Empty);

        Entry? newEntry = context.Catalog.Children.SingleOrDefault(entry=>entry.Path == fileName);
        if (newEntry == null)
        {
            throw new UserErrorException("Directory was removed");
        }
        
        // now selecting new entry
        context.Panel!.IsFocused = true;
        context.Panel!.SelectedEntry = newEntry;
    }
}