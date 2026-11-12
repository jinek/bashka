namespace Bashka.Plugins.Core.Dialog.Basic;

public class ErrorDialogContent : OkCancelDialogContentBase
{
    public ErrorDialogContent(string errorText) : base("Error", new ErrorDialog(errorText))
    {
        Classes = Classes.Concat(new[] { "error_background" }).ToArray();
    }
}