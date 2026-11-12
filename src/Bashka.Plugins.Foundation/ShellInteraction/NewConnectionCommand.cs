using Bashka.Plugins.Core.Abstract.Data;
using Bashka.Plugins.Core.Dialog.Abstract;
using Bashka.Plugins.Core.Dialog.Basic;
using Bashka.Plugins.Core.FunctionKeys;
using Bashka.Plugins.Core.Scoping;

namespace Bashka.Plugins.Foundation.ShellInteraction;

public class NewConnectionCommand(IDialogProvider dialogProvider)
    : TypeKeyedObject, IFunctionBarCommand<ConnectionsCatalog>
{
    public bool CanExecute(CommandExecutionContext context) => true;

    public async Task ExecuteAsync(CommandExecutionContext context)
    {
        string title = await dialogProvider.ShowOkCancelDialogAsync(
            new StringInputDialog("Enter connection name", validationMode: InputValidationMode.FileName));

        var catalog = (ConnectionsCatalog)context.Catalog!;
        string filePath = Path.Combine(ConnectionsCatalog.ConnectionsDirectory, $"{title}.ini");

        SshConnectionSettings.CreateDefault(filePath);

        await NanoLauncher.OpenAsync(catalog, ConnectionsCatalog.ConnectionsDirectory, filePath);

        await catalog.NavigateAsync(string.Empty);
    }
}
