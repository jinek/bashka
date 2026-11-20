using Autofac;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Bashka.Plugins.Core.Abstract;
using Bashka.Plugins.Core.Abstract.Data;
using PropertyChanged;

namespace Bashka.Plugins.Core.Orthodox.UI;

/// <summary>
///     Executes the command registered with <see cref="CommandKey" />, like a key binding does
/// </summary>
[DoNotNotify]
public class CommandMenuItem : MenuItem
{
    public string CommandKey { get; set; } = string.Empty;

    protected override Type StyleKeyOverride => typeof(MenuItem);

    protected override async void OnClick(RoutedEventArgs e)
    {
        base.OnClick(e);
        var commander = (OrthodoxCommander)DataContext!;
        var command = commander.LifetimeScope.ResolveKeyed<ICommand>(CommandKey);
        var context = new CommandExecutionContext(commander);
        if (command.CanExecute(context))
            await command.ExecuteAsync(context);
    }
}