using Avalonia.Controls;
using Avalonia.Xaml.Interactivity;
using PropertyChanged;

namespace Bashka.Plugins.Core.Orthodox.UI.Helpers;

[DoNotNotify]
// ReSharper disable once UnusedType.Global May be will use soon
public class PagingScrollBehavior : Behavior<VerticalScrollDataGrid>
{
    private bool _isScrolling;

    protected override void OnAttached()
    {
        base.OnAttached();

        AssociatedObject!.SelectionChanged += OnSelectionChanged;
    }

    protected override void OnDetaching()
    {
        base.OnDetaching();
        AssociatedObject!.SelectionChanged -= OnSelectionChanged;
    }


    private void OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (sender is not VerticalScrollDataGrid grid || _isScrolling || grid.ScrollBar is null) return;
        //if one row is not one pixel, then ratio can be calculated
        double viewPortSize = grid.ScrollBar.ViewportSize;
        double maximum = grid.ScrollBar.Maximum;
        double scrollPosition = grid.ScrollBar.Value;
        double selectedIndex = grid.SelectedIndex;

        var items = (IEnumerable<object>)grid.ItemsSource;

        int? targetIndex = null;

        if (selectedIndex - scrollPosition > viewPortSize * 0.9)
            targetIndex = (int)Math.Min(selectedIndex + (int)(0.8 * viewPortSize), maximum + viewPortSize - 1);
        else if (selectedIndex - scrollPosition < viewPortSize * 0.1)
            targetIndex = (int)Math.Max(selectedIndex - (int)(0.8 * viewPortSize), 0);
        if (targetIndex == null) return;

        _isScrolling = true;
        grid.ScrollIntoView(items.ElementAt((int)targetIndex), null);
        _isScrolling = false;
    }
}