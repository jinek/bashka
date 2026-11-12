namespace Bashka.Plugins.Core.Dialog.Abstract;

public interface IDialogContent<out TResult>
{
    Type TemplateKey => GetType();
    event Action<TResult> OnClose;
    string Title { get; }
    IReadOnlyList<string> Classes { get; }
}