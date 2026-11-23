using Autofac.Features.Indexed;
using Bashka.Helpers;
using Bashka.Plugins.Core.Abstract;
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

public class TouchFileCommand : TypeKeyedObject, IFunctionBarCommand<ShellCatalog>
{
    private readonly IIndex<string, ICommand> _commandFactory;
    private readonly IDialogProvider _dialogProvider;
    private readonly WorkerTaskFactory _workerTaskFactory;

    public TouchFileCommand(IDialogProvider dialogProvider, WorkerTaskFactory workerTaskFactory,
        IIndex<string, ICommand> commandFactory)
    {
        _dialogProvider = dialogProvider;
        _workerTaskFactory = workerTaskFactory;
        _commandFactory = commandFactory;
    }

    public bool CanExecute(CommandExecutionContext context)
    {
        return true;
    }

    public async Task ExecuteAsync(CommandExecutionContext context)
    {
        context.Panel!.SelectedIndex = -1;

        string fileName = await _dialogProvider.ShowOkCancelDialogAsync(new StringInputDialog("Enter file name"));

        await _workerTaskFactory.StartNewWorkerTask(async (_, _, _) =>
        {
            string path = BashkaHelpers.PathJoin(context.Catalog!.CurrentPath!, fileName);
            ((ShellCatalog)context.Catalog)
                .ShellSession
                .ExecuteNonQuery(new TouchFileMultiScript(path));
        });

        await context.Catalog!.NavigateAsync(string.Empty);

        Entry? newEntry = context.Catalog.Children.SingleOrDefault(entry => entry.Path == fileName);
        if (newEntry == null) throw new UserErrorException("File was removed");

        ICommand editCommand = _commandFactory[ICommand.EditCommandKey];
        await editCommand.ExecuteAsync(context with { Entries = [newEntry] });

        // now selecting new entry
        context.Panel!.IsFocused = true;
        context.Panel!.SelectedEntry = newEntry;
    }
}