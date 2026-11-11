using System.Reactive;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Input;
using Avalonia.Xaml.Interactivity;
using PropertyChanged;

namespace Bashka.Plugins.Core.Orthodox.UI.Helpers;

[DoNotNotify]
public class OrthodoxMenuBehavior : Behavior<Menu>
{
    private IDisposable? _subscription;

    protected override void OnAttached()
    {
        base.OnAttached();

        AssociatedObject!.DataContextChanged += OnDataContextChanged;
        SubscribeToInteraction();
    }

    protected override void OnDetaching()
    {
        base.OnDetaching();

        AssociatedObject!.DataContextChanged -= OnDataContextChanged;
        _subscription?.Dispose();
        _subscription = null;
    }

    private void OnDataContextChanged(object? sender, EventArgs e)
    {
        SubscribeToInteraction();
    }

    private void SubscribeToInteraction()
    {
        _subscription?.Dispose();
        _subscription = null;

        if (AssociatedObject!.DataContext !=null)
        {
            OrthodoxCommander commander = (OrthodoxCommander)AssociatedObject!.DataContext;
            _subscription = commander!.FocusMenuInteraction.RegisterHandler(interaction =>
            {
                FocusMenu(commander);
                interaction.SetOutput(Unit.Default);
            });
        }
    }

    private void FocusMenu(OrthodoxCommander commander)
    {
        IEnumerable<MenuItem> menuItems = AssociatedObject.Items.OfType<MenuItem>();

        MenuItem targetItem = menuItems.Single(item =>
            commander.CurrentPanel == commander.LeftPanel
                ? item.Name == "PART_LeftMenuItem"
                : item.Name == "PART_RightMenuItem");
        
        targetItem.Focus(NavigationMethod.Directional);
    }
}
