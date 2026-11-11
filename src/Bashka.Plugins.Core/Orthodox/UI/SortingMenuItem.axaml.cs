using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Bashka.Plugins.Core.Abstract;
using Bashka.Plugins.Core.Abstract.Data;
using PropertyChanged;

namespace Bashka.Plugins.Core.Orthodox.UI;

[DoNotNotify]
public partial class SortingMenuItem : MenuItem
{
    public SortingMenuItem()
    {
        InitializeComponent();
    }

    protected override Type StyleKeyOverride => typeof(MenuItem);

    private void MenuItem_OnAttachedToVisualTree(object sender, VisualTreeAttachmentEventArgs _)
    {
        var panel = (IPanel)DataContext!;
        var menuItem = ((Visual)sender).FindAncestorOfType<MenuItem>();
        bool isCurrent = (SortMode)menuItem.DataContext! == panel.SortMode;
        menuItem.IsChecked = isCurrent;
        if (isCurrent)
        {
            Dispatcher.Post(() =>
            {
                menuItem.Focus();
            },DispatcherPriority.ApplicationIdle);
            
        }

        menuItem.Click += MenuItemOnKeyDown;
    }

    private void MenuItemOnKeyDown(object sender, RoutedEventArgs e)
    {
        SortingSelected(sender);
    }

    private void SortingSelected(object sender)
    {
        var panel = (IPanel)DataContext!;
        panel.SortMode = (SortMode)((Visual)sender).DataContext!;
    }

    private void MenuItem_OnDetachedFromVisualTree(object sender, VisualTreeAttachmentEventArgs e)
    {
        var menuItem = ((Visual)sender).FindAncestorOfType<MenuItem>();
        menuItem.Click -= MenuItemOnKeyDown;
    }
}