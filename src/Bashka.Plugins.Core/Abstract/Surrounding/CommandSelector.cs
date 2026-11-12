using Autofac.Features.AttributeFilters;
using Bashka.Plugins.Core.Abstract.Data;

namespace Bashka.Plugins.Core.Abstract.Surrounding;

public class CommandSelector : ICommandSelector
{
    private readonly IEnumerable<ICommand> _commands;

    public CommandSelector([KeyFilter(ICommand.EnterCommandKey)] IEnumerable<ICommand> commands)
    {
        _commands = commands;
    }

    public ICommand[] SelectCommands(CommandExecutionContext context)
    {
        return _commands.Where(command => command.CanExecute(context)).ToArray();
    }
}