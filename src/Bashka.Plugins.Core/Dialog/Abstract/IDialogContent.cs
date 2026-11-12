namespace Bashka.Plugins.Core.Dialog.Abstract;

public interface IDialogContent<out TResult>
{
    Type TemplateKey => GetType();
    string Title { get; }
    IReadOnlyList<string> Classes { get; }
    event Action<TResult> OnClose;
}