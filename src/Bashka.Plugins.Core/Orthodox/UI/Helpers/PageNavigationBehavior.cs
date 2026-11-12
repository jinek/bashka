using Avalonia.Input;
using Avalonia.Xaml.Interactivity;
using PropertyChanged;

namespace Bashka.Plugins.Core.Orthodox.UI.Helpers
{
    /// <summary>
    /// Makes LEFT arrow work as PageUp and RIGHT arrow as PageDown while the panel grid is focused.
    /// this is neuro-generated, I could not do better
    /// </summary>
    [DoNotNotify]
    public class PageNavigationBehavior : Behavior<VerticalScrollDataGrid>
    {
        protected override void OnAttached()
        {
            base.OnAttached();

            AssociatedObject!.AddHandler(InputElement.KeyDownEvent, OnKeyDown, Avalonia.Interactivity.RoutingStrategies.Tunnel);
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();

            AssociatedObject!.RemoveHandler(InputElement.KeyDownEvent, OnKeyDown);
        }

        private void OnKeyDown(object? sender, KeyEventArgs e)
        {
            if (e.KeyModifiers != KeyModifiers.None)
                return;

            int direction = e.Key switch
            {
                Key.Left => -1,
                Key.Right => 1,
                _ => 0
            };

            if (direction == 0)
                return;

            VerticalScrollDataGrid grid = AssociatedObject!;
            if (grid.ItemsSource is not IEnumerable<object> items)
                return;

            IEnumerable<object> array = items as object[] ?? [.. items];
            int count = array.Count();
            if (count == 0)
                return;

            int pageSize = GetPageSize(grid);
            int targetIndex = Math.Clamp(grid.SelectedIndex + direction * pageSize, 0, count - 1);

            
            grid.SelectedIndex = targetIndex;
            grid.ScrollIntoView(array.ElementAt(targetIndex), grid.Columns.First());

            e.Handled = true;
        }

        private static int GetPageSize(VerticalScrollDataGrid grid)
        {
            double viewPortSize = grid.ScrollBar!.ViewportSize;
            return Math.Max(1, (int)viewPortSize - 1);
        }
    }
}
