using Bashka.Plugins.Core.Scoping;

namespace Bashka.Plugins.Core.Dialog.Abstract;

public class DialogContent<TResult> : BaseObject, IDialogContent<TResult>
{
    protected DialogContent(string title, params string[] classes)
    {
        Title = title;
        Classes = classes;
    }

    public event Action<TResult>? OnClose;
    public string Title { get; }
    public string[] Classes { get; protected set; } //todo: make readonly

    protected virtual void RaiseClose(TResult result)
    {
        OnClose?.Invoke(result);
    }
}

public class DialogContent : DialogContent<object?>
{
    protected DialogContent(string title, params string[] classes) : base(title, classes)
    {
    }

    protected sealed override void RaiseClose(object? result)
    {
        // Don't remember how to hide method
        throw new NotSupportedException("Use RaiseClose() instead");
    }

    protected void RaiseClose()
    {
        base.RaiseClose(null);
    }
}