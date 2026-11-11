using Bashka.Plugins.Core.Abstract.Data;

namespace Bashka.Plugins.Core.Abstract;

public interface ICommandSelector
{
    /// <summary>
    /// Commands which can be executed for the selected entry (by Enter),
    /// i.e. registered with <see cref="ICommand.EnterCommandKey"/>.
    /// </summary>
    ICommand[] SelectCommands(CommandExecutionContext context);
}
