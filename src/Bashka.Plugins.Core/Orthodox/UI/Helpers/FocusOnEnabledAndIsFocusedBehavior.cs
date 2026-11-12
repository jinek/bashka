using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using Avalonia.Xaml.Interactivity;
using Bashka.Plugins.Core.Abstract;
using PropertyChanged;

namespace Bashka.Plugins.Core.Orthodox.UI.Helpers;

[DoNotNotify]
public class FocusOnEnabledAndIsFocusedBehavior : Behavior<DataGrid>
{
    private bool IsFocused => AssociatedObject?.DataContext is IPanel { IsFocused: true };

    protected override void OnAttachedToVisualTree()
    {
        AssociatedObject!.PropertyChanged += OnAssociatedObjectOnPropertyChanged;
        /*_subscribe = AssociatedObject.WhenPropertyChanged(control => control.IsEffectivelyEnabled)
            .Subscribe(b =>
            {
                if (b.Value && IsFocused)
                    AssociatedObject?.Focus();
            });*/
        base.OnAttachedToVisualTree();
    }

    private void OnAssociatedObjectOnPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs args)
    {
        if (args.Property == InputElement.IsEffectivelyEnabledProperty)
        {
            if ((bool)args.NewValue! && IsFocused)
            {
                ThreadPool.QueueUserWorkItem(_ =>
                {
                    Dispatcher.UIThread.Post(() =>
                    {
                        if(AssociatedObject!.SelectedIndex<=0)
                            AssociatedObject.SelectedIndex = 0;
                        AssociatedObject?.Focus();
                    });
                });
                
            }
        }
    }

    protected override void OnDetachedFromVisualTree()
    {
        AssociatedObject!.PropertyChanged -= OnAssociatedObjectOnPropertyChanged;
        base.OnDetachedFromVisualTree();
    }
}