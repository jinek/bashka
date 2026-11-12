using Bashka.Plugins.Core.Dialog.Abstract;

namespace Bashka.Plugins.Core.Dialog;

public static class DialogContentExtensions
{
    public static Task CreateDialog(this IDialogProvider dialogProvider, DialogContent dialogContent)
    {
        return dialogProvider.ShowDialogAsync(dialogContent);
    }
}