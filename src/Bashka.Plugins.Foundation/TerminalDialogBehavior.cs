using System.Reflection;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Avalonia.Xaml.Interactivity;
using Iciclecreek.Terminal;
using PropertyChanged;

namespace Bashka.Plugins.Foundation;

[DoNotNotify]
public sealed class TerminalDialogBehavior : Behavior<TerminalControl>
{
    static TerminalDialogBehavior()
    {
        // Consolonia's cmap maps every character to glyph 1; text must go through its shaper.
        typeof(TerminalView).GetField("GlyphRunFastPathEnabled", BindingFlags.NonPublic | BindingFlags.Static)!
            .SetValue(null, false);
    }

    protected override void OnAttachedToVisualTree()
    {
        base.OnAttachedToVisualTree();
        AssociatedObject!.ProcessExited += OnProcessExited;
        AssociatedObject.Loaded += OnLoaded;
    }

    protected override void OnDetachedFromVisualTree()
    {
        AssociatedObject!.ProcessExited -= OnProcessExited;
        AssociatedObject.Loaded -= OnLoaded;
        AssociatedObject.Dispose();
        base.OnDetachedFromVisualTree();
    }

    private void OnLoaded(object? sender, RoutedEventArgs e)
    {
        AssociatedObject!.Focus();
    }

    private void OnProcessExited(object? sender, ProcessExitedEventArgs e)
    {
        Dispatcher.UIThread.Post(() => (AssociatedObject?.DataContext as TerminalDialogContent)?.Close());
    }
}