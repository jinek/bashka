using Bashka.Plugins.Core.Abstract.Data;
using Bashka.Plugins.Core.Dialog.Abstract;
using Bashka.Plugins.Core.Dialog.Basic;
using Bashka.Plugins.Core.FunctionBar;

namespace Bashka.Plugin.Template;

public sealed class HelloWorldCommand(IDialogProvider dialogProvider) : IFunctionBarCatalogCommand
{
    public bool CanExecute(CommandExecutionContext context) => true;

    public Task ExecuteAsync(CommandExecutionContext context)
    {
        return dialogProvider.ShowDialogAsync(new OkCancelQuestionDialogContent("Hello", "Hello world"));
    }
}
