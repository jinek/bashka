using Autofac.Features.Indexed;
using Bashka.Plugins.Core.Abstract;
using Bashka.Plugins.Core.Abstract.Data;
using Bashka.Plugins.Core.Dialog.Abstract;
using Bashka.Plugins.Core.Dialog.Basic;
using Bashka.Plugins.Core.FunctionKeys;
using Bashka.Plugins.Core.Scoping;

namespace Bashka.Plugins.Foundation.ShellInteraction;

public class NewConnectionCommand(IDialogProvider dialogProvider, IIndex<string, ICommand> commands)
    : TypeKeyedObject, IFunctionBarCommand<ConnectionsCatalog>
{
    public bool CanExecute(CommandExecutionContext context)
    {
        return true;
    }

    public async Task ExecuteAsync(CommandExecutionContext context)
    {
        ICommand editCommand = commands[ICommand.EditCommandKey];

        string title = await dialogProvider.ShowOkCancelDialogAsync(
            new StringInputDialog("Enter connection name", validationMode: InputValidationMode.FileName));

        var catalog = (ConnectionsCatalog)context.Catalog!;
        string filePath = Path.Combine(ConnectionsCatalog.ConnectionsDirectory, $"{title}.ini");

        SshConnectionSettings.CreateDefault(filePath);

        await catalog.NavigateAsync(string.Empty);
        SwitchConnectionEntry entry = catalog.Children.OfType<SwitchConnectionEntry>()
            .Single(connection => connection.FullPath == filePath);
        await editCommand.ExecuteAsync(context with { Entries = [entry] });

        await catalog.NavigateAsync(string.Empty);
    }
}
