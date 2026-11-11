using Bashka.Plugins.Core.Abstract.Data;

namespace Bashka.Plugins.Core.Abstract;

public interface ICommand
{
    /// <summary>
    /// Key under which a command executable for the selected entry (by Enter) is registered.
    /// Only such commands are selected by <see cref="ICommandSelector.SelectCommands"/>.
    /// </summary>
    public const string EnterCommandKey = "EnterCommand";

    public bool CanExecute(CommandExecutionContext context);
    public Task ExecuteAsync(CommandExecutionContext context);
}
