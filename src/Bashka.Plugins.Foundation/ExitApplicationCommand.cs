using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Bashka.Plugins.Core.Abstract.Data;
using Bashka.Plugins.Core.Dialog.Abstract;
using Bashka.Plugins.Core.Dialog.Basic;
using Bashka.Plugins.Core.FunctionKeys;

namespace Bashka.Plugins.Foundation;

public class ExitApplicationCommand : IFunctionBarCatalogCommand
{
    private readonly IDialogProvider _dialogProvider;

    public ExitApplicationCommand(IDialogProvider dialogProvider)
    {
        _dialogProvider = dialogProvider;
    }

    public bool CanExecute(CommandExecutionContext context)
    {
        return true;
    }

    public async Task ExecuteAsync(CommandExecutionContext context)
    {
        var dialogContent = new OkCancelQuestionDialogContent("Exit", "Are you sure you want to exit?");

        if (await _dialogProvider.ShowDialogAsync(dialogContent))
            ((IControlledApplicationLifetime)Application.Current!.ApplicationLifetime!).Shutdown();
    }
}