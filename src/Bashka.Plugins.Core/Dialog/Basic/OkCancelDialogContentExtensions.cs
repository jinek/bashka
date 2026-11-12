using Bashka.Plugins.Core.Abstract.Surrounding;
using Bashka.Plugins.Core.Dialog.Abstract;

namespace Bashka.Plugins.Core.Dialog.Basic;

public static class OkCancelDialogContentExtensions
{
    public static async Task<T> ShowOkCancelDialogAsync<T>(this IDialogProvider dialogProvider,
        DialogWithResultBase<T> content)
    {
        var okCancelDialogContentBase = new OkCancelDialogContentBase("Title", content);
        bool isOk = await dialogProvider.ShowDialogAsync(okCancelDialogContentBase);
        return isOk ? content.Result! : throw new UserCancelException();
    }
}