using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Threading;
using Avalonia.Xaml.Interactivity;
using PropertyChanged;

namespace Bashka.Plugins.Core.Orthodox.UI.Helpers;

[DoNotNotify]
public class FocusSelectedItemIfAnyViaDispatcherAction : AvaloniaObject, IAction
{
    public object Execute(object? sender, object? parameter)
    {
        var selectingItemsControl = (SelectingItemsControl)sender!;

        Dispatcher.UIThread.Post(() =>
        {
            int selectedIndex = selectingItemsControl.SelectedIndex;
            if (selectedIndex < 0)
                return;

            Control? container = selectingItemsControl.ContainerFromIndex(selectedIndex);
            if (container == null)
                return;

            container.Focus(NavigationMethod.Directional);
        }, DispatcherPriority.Background);

        return true;
    }
}