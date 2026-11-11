using Bashka.Plugins.Core.Abstract.Data;
using Bashka.Plugins.Core.Dialog.Abstract;
using Bashka.Plugins.Core.FunctionBar;

namespace Bashka.Plugins.Foundation;

public class AboutCatalogCommand : IFunctionBarCatalogCommand
{
    private readonly IDialogProvider _dialogProvider;

    public AboutCatalogCommand(IDialogProvider dialogProvider)
    {
        _dialogProvider = dialogProvider;
    }

    public bool CanExecute(CommandExecutionContext context)
    {
        return true;
    }

    public Task ExecuteAsync(CommandExecutionContext context)
    {
        return _dialogProvider.ShowDialogAsync(new AboutDialogContent());
    }
}