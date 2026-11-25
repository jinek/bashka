using Bashka.Plugins.Core.Abstract;
using Bashka.Plugins.Core.Abstract.Data;
using JetBrains.Annotations;

namespace Bashka.Plugins.Core.Orthodox;

[UsedImplicitly]
public class SwitchPanelCommand : ICommand
{
    public bool CanExecute(CommandExecutionContext context)
    {
        return context.AnotherPanel != null;
    }

    public Task ExecuteAsync(CommandExecutionContext context)
    {
        context.AnotherPanel!.IsFocused = true;
        return Task.CompletedTask;
    }
}