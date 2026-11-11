using System.Reflection.PortableExecutable;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Xaml.Interactivity;
using Bashka.Plugins.Core.Abstract;
using PropertyChanged;

namespace Bashka.Plugins.Core.Orthodox.UI.Helpers;

[DoNotNotify]
public class QuickSearchBehavior : Behavior<TextBox>
{
    protected override void OnAttached()
    {
        AssociatedObject!.AddHandler(InputElement.KeyDownEvent, OnKeyDown, handledEventsToo: true);
        AssociatedObject!.AddHandler(InputElement.GotFocusEvent, OnGotFocus);
    }

    private void OnGotFocus(object? sender, FocusChangedEventArgs e)
    {
        AssociatedObject!.CaretIndex = AssociatedObject!.Text!.Length;
    }

    protected override void OnDetaching() =>
        AssociatedObject!.RemoveHandler(InputElement.KeyDownEvent, OnKeyDown);

    private void OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (AssociatedObject?.DataContext is not IPanel panel) return;

        switch (e.Key)
        {
            case Key.Escape:
                panel.IsQuickSearchActive = false;
                e.Handled = true;
                break;
            case Key.Enter:
                if (panel.SelectedEntry?.IsDirectory ?? false)
                    panel.EntrySelected();
                else
                    panel.IsQuickSearchActive = false;
                e.Handled = true;
                break;
            case Key.Down:
                panel.QuickSearchNext();
                e.Handled = true;
                break;
            case Key.Tab:
                panel.IsQuickSearchActive = false;
                break;
        }
    }
}
