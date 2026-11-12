using Bashka.Plugins.Core.Abstract.Data;

namespace Bashka.Plugins.Core.Abstract;

public interface ICommand
{
    /// <summary>
    ///     Key under which a command executable for the selected entry (by Enter) is registered.
    ///     Only such commands are selected by <see cref="ICommandSelector.SelectCommands" />.
    /// </summary>
    const string EnterCommandKey = "EnterCommand";

    bool CanExecute(CommandExecutionContext context);
    Task ExecuteAsync(CommandExecutionContext context);
}