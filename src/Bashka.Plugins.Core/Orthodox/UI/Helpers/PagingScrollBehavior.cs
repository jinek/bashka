using Avalonia.Controls;
using Avalonia.Xaml.Interactivity;
using PropertyChanged;

namespace Bashka.Plugins.Core.Orthodox.UI.Helpers
{
    [DoNotNotify]
    public class PagingScrollBehavior : Behavior<VerticalScrollDataGrid>
    {
        private bool _isScrolling;
        protected override void OnAttached()
        {
            base.OnAttached();
        
            AssociatedObject.SelectionChanged += OnSelectionChanged;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            AssociatedObject.SelectionChanged -= OnSelectionChanged;
        }


        private void OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            if (sender is not VerticalScrollDataGrid grid || _isScrolling || grid._scrollBar is null)
            {
                return;
            }
            //if one row is not one pixel, then ratio can be calculated
            double viewPortSize = grid._scrollBar.ViewportSize;
            double maximum = grid._scrollBar.Maximum;
            double scrollPosition = grid._scrollBar.Value;
            double selectedIndex = grid.SelectedIndex;
        
            var items = (IEnumerable<object>)grid.ItemsSource;

            int? targetIndex = null;

            if (selectedIndex - scrollPosition > viewPortSize * 0.9)
            {
                targetIndex = (int)Math.Min(selectedIndex + (int)(0.8 * viewPortSize), maximum + viewPortSize - 1);
            }
            else if (selectedIndex - scrollPosition < viewPortSize * 0.1)
            {
                targetIndex = (int)Math.Max(selectedIndex - (int)(0.8 * viewPortSize), 0);
            }
            if (targetIndex == null) return;
        
            _isScrolling = true;
            grid.ScrollIntoView(items.ElementAt((int)targetIndex), null);
            _isScrolling = false;
        }
    }
}