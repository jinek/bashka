using Bashka.Plugins.Core.Dialog.Abstract;

namespace Bashka.Plugins.Core.Dialog.Basic;

public class OkCancelDialogContentBase : DialogContent<bool>, IDialogContent<bool>
{
    public OkCancelDialogContentBase(string title, DialogBase content) : base(title, "centered", "min_size",
        "normal_background")
    {
        Content = content;
        ContentType = content.GetType();
    }

    public DialogBase Content { get; }
    public Type ContentType { get; }

    public void Ok()
    {
        RaiseClose(true);
    }

    public void Cancel()
    {
        RaiseClose(false);
    }

    Type IDialogContent<bool>.TemplateKey => typeof(OkCancelDialogContentBase);
}