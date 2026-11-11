namespace Bashka.Plugins.Core.Dialog.Basic
{
    public class ErrorDialogContent : OkCancelDialogContentBase
    {
        public ErrorDialogContent(string errorText) : base("Error", new ErrorDialog(errorText))
        {
            Classes = Enumerable.Concat(Classes, new[] { "error_background" }).ToArray();
        }
    }
}