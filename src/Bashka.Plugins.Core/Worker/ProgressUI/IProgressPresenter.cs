using Avalonia.Collections;
using Bashka.Plugins.Core.Dialog.Abstract;

namespace Bashka.Plugins.Core.Worker.ProgressUI;

public interface IProgressPresenter : IDialogContent<object?>
{
    void ShowProgress(ProgressViewModel viewModel);
    void HideProgress(ProgressViewModel viewModel);
    AvaloniaList<ProgressViewModel> ProgressViewModels { get; }

    Type IDialogContent<object?>.TemplateKey => typeof(IProgressPresenter);
}