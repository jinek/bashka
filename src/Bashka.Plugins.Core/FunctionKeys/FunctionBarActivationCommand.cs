using Bashka.Plugins.Core.Abstract;
using Bashka.Plugins.Core.Abstract.Data;

namespace Bashka.Plugins.Core.FunctionKeys;

public class FunctionBarActivationCommand : ICommand
{
    private readonly FunctionPage _functionPage;
    private readonly FunctionKey _functionKey;

    public FunctionBarActivationCommand(FunctionPage functionPage, FunctionKey functionKey)
    {
        _functionPage = functionPage;
        _functionKey = functionKey;
    }

    private FunctionBarItem? GetFunctionBarItem(CommandExecutionContext context)
    {
        FunctionPage functionPage = _functionPage == FunctionPage.Default
            ? context.OrthodoxCommander!.FunctionBar.CurrentPageIndex
            : _functionPage;

        return context.OrthodoxCommander!.FunctionBar.AllPages[(int)functionPage].Items[(int)_functionKey];
    }

    public bool CanExecute(CommandExecutionContext context)
    {
        if (context.OrthodoxCommander == null) return false;

        FunctionBarItem? toolBoxItem = GetFunctionBarItem(context);
         return toolBoxItem?.CurrentCommandWrapper.Command?.CanExecute(context) == true;
    } 

    public async Task ExecuteAsync(CommandExecutionContext context)
    {
        FunctionBarItem toolBoxItem = GetFunctionBarItem(context)!;
        ICommand command = toolBoxItem.CurrentCommandWrapper.Command!;

        await command.ExecuteAsync(context);

        if (command is not FunctionPageChangeCatalogCommand)
        {
            context.OrthodoxCommander!.FunctionBar.CurrentPageIndex = FunctionPage.Default;
        }
    }
}