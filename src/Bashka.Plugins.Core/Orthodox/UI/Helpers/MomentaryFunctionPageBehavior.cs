using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Xaml.Interactivity;
using Bashka.Plugins.Core.FunctionKeys;
using PropertyChanged;

namespace Bashka.Plugins.Core.Orthodox.UI.Helpers
{
    /// <summary>
    /// Some terminals (kitty) report solo modifier keys. While such a key is held down, the function bar
    /// shows the matching page, so the user can see what ALT+F or CTRL+F would do.
    /// </summary>
    [DoNotNotify]
    public class MomentaryFunctionPageBehavior : Behavior<Control>
    {
        private FunctionPage? _pageBeforeMomentary;
        private Key? _momentaryKey;

        protected override void OnAttached()
        {
            base.OnAttached();

            AssociatedObject!.AddHandler(InputElement.KeyDownEvent, OnKeyDown, RoutingStrategies.Tunnel);
            AssociatedObject!.AddHandler(InputElement.KeyUpEvent, OnKeyUp, RoutingStrategies.Tunnel);
            AssociatedObject!.AddHandler(InputElement.LostFocusEvent, OnLostFocus, RoutingStrategies.Bubble);
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();

            AssociatedObject!.RemoveHandler(InputElement.KeyDownEvent, OnKeyDown);
            AssociatedObject!.RemoveHandler(InputElement.KeyUpEvent, OnKeyUp);
            AssociatedObject!.RemoveHandler(InputElement.LostFocusEvent, OnLostFocus);
        }

        private static FunctionPage? GetMomentaryPage(KeyEventArgs e)
        {
            (FunctionPage page, KeyModifiers ownModifier)? modifier = e.Key switch
            {
                Key.LeftCtrl or Key.RightCtrl => (FunctionPage.Ctrl, KeyModifiers.Control),
                Key.LeftAlt or Key.RightAlt => (FunctionPage.Alt, KeyModifiers.Alt),
                Key.LeftShift or Key.RightShift => (FunctionPage.Shift, KeyModifiers.Shift),
                _ => null
            };

#pragma warning disable CA1508 // todo: low what??? why this error here????
            if (modifier == null) return null;
#pragma warning restore CA1508

            // solo modifier only, so ALT+CTRL is not treated as a page switch
            KeyModifiers otherModifiers = e.KeyModifiers & ~modifier.Value.ownModifier;
            return otherModifiers == KeyModifiers.None ? modifier.Value.page : null;
        }

        private FunctionKeys.FunctionBar? FunctionBar =>
            (AssociatedObject?.DataContext as OrthodoxCommander)?.FunctionBar;

        private void OnKeyDown(object? sender, KeyEventArgs e)
        {
            FunctionPage? page = GetMomentaryPage(e);
            if (page == null) return;

            FunctionKeys.FunctionBar? functionBar = FunctionBar;
            if (functionBar == null) return;

            if (_momentaryKey == e.Key) return;

            _pageBeforeMomentary ??= functionBar.CurrentPageIndex;
            _momentaryKey = e.Key;
            functionBar.CurrentPageIndex = page.Value;
        }

        private void OnKeyUp(object? sender, KeyEventArgs e)
        {
            if (_momentaryKey != e.Key) return;

            RestorePage();
        }

        private void OnLostFocus(object? sender, RoutedEventArgs e) => RestorePage();

        private void RestorePage()
        {
            if (_pageBeforeMomentary == null) return;

            FunctionKeys.FunctionBar? functionBar = FunctionBar;
            if (functionBar != null)
                functionBar.CurrentPageIndex = _pageBeforeMomentary.Value;

            _pageBeforeMomentary = null;
            _momentaryKey = null;
        }
    }
}
