using Bashka.Plugins.Core.Abstract;
using Bashka.Plugins.Core.Abstract.Data;

namespace Bashka.Plugins.Core.Orthodox;

/// <summary>
///     Changes marks of all entries of the current catalog (Far manager style Gray +/-/*)
/// </summary>
public abstract class ChangeMarksCommandBase : ICommand
{
    public bool CanExecute(CommandExecutionContext context)
    {
        return context.Catalog is { AllowsMultipleSelection: true };
    }

    public Task ExecuteAsync(CommandExecutionContext context)
    {
        foreach (Entry entry in context.Catalog!.Children.Where(entry => !entry.IsGone))
            entry.IsMarked = GetNewValue(entry.IsMarked);

        return Task.CompletedTask;
    }

    protected abstract bool GetNewValue(bool currentValue);
}

public class MarkAllCommand : ChangeMarksCommandBase
{
    protected override bool GetNewValue(bool currentValue)
    {
        return true;
    }
}

public class UnmarkAllCommand : ChangeMarksCommandBase
{
    protected override bool GetNewValue(bool currentValue)
    {
        return false;
    }
}

public class InvertMarksCommand : ChangeMarksCommandBase
{
    protected override bool GetNewValue(bool currentValue)
    {
        return !currentValue;
    }
}