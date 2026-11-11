using Bashka.Plugins.Core.Abstract.Data;
using Bashka.Plugins.Core.Scoping;

namespace Bashka.Plugins.Core.FunctionBar
{
    public class FunctionPageChangeCatalogCommand : BaseObject, IFunctionBarCatalogCommand
    {
        public bool CanExecute(CommandExecutionContext context)
        {
            return true;
        }

        public async Task ExecuteAsync(CommandExecutionContext context)
        {
            FunctionPage toolBoxCurrentPageIndex = context.OrthodoxCommander!.FunctionBar.CurrentPageIndex;
            if (toolBoxCurrentPageIndex == FunctionPage.Shift)
            {
                context.OrthodoxCommander!.FunctionBar.CurrentPageIndex = 0;
                return;
            }

            context.OrthodoxCommander!.FunctionBar.CurrentPageIndex++;
        }
    }
}