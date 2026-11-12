using Bashka.Plugins.Core.Abstract.Data;
using Bashka.Plugins.Core.Dialog.Abstract;
using Bashka.Plugins.Core.FunctionKeys;

namespace Bashka.Plugins.Foundation;

public class AboutCatalogCommand(IDialogProvider dialogProvider) : IFunctionBarCatalogCommand
{
    public bool CanExecute(CommandExecutionContext context)
    {
        return true;
    }

    public Task ExecuteAsync(CommandExecutionContext context)
    {
        return dialogProvider.ShowDialogAsync(new AboutDialogContent());
    }
}