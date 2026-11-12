using Bashka.Plugins.Core.Abstract;
using Bashka.Plugins.Core.Abstract.Data;
using Bashka.Plugins.Core.Abstract.Surrounding;

namespace Bashka.Plugins.Core.Orthodox;

/// <summary>
///     Toggles the mark of the focused entry and moves the cursor (Far manager style, e.g. Insert or Shift+Down/Up)
/// </summary>
public class ToggleMarkCommand(int moveDelta) : ICommand
{
    public bool CanExecute(CommandExecutionContext context)
    {
        if (context.Catalog is not { AllowsMultipleSelection: true })
            return false;

        return context.Panel?.SelectedEntry is { IsGone: false } and not GoBackEntry;
    }

    public Task ExecuteAsync(CommandExecutionContext context)
    {
        IPanel panel = context.Panel!;
        Entry entry = panel.SelectedEntry!;
        entry.IsMarked = !entry.IsMarked;
        int newIndex = panel.SelectedIndex + moveDelta;
        if (newIndex >= 0 && newIndex < (panel.CurrentChildren?.Count() ?? 0))
            panel.SelectedIndex = newIndex;

        return Task.CompletedTask;
    }
}