using Bashka.Plugins.Core.Dialog.Abstract;

namespace Bashka.Plugins.Core.Dialog.Basic;

public class ErrorDialog : DialogWithResultBase<object>
{
    public ErrorDialog(string error)
    {
        Error = error;
        IsOkVisible = false;
        CancelText = "Close";
    }

    public string Error { get; }
}