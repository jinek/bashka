namespace Bashka.Plugins.Core.Dialog.Abstract;

public interface IDialogProvider
{
    Task<TResult?> ShowDialogAsync<TResult>(IDialogContent<TResult> dialogContent);
}