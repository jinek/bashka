using Avalonia.Collections;
using Avalonia.Threading;
using Bashka.Helpers;
using Bashka.Plugins.Core.Dialog.Abstract;

namespace Bashka.Plugins.Core.Worker.ProgressUI;

public class ProgressPresenter : DialogContent, IProgressPresenter
{
    private readonly IDialogProvider _dialogProvider;

    public ProgressPresenter(
        IDialogProvider dialogProvider /*, [KeyFilter(TaskFactoryHelper.UITaskFactoryKey)] TaskFactory iuiTaskFactory*/)
        : base("Processing", "centered", "min_size", "negative_padding")
    {
        _dialogProvider = dialogProvider;
        //_uiTaskFactory = iuiTaskFactory;
    }

    public AvaloniaList<ProgressViewModel> ProgressViewModels { get; } = new();


    public void ShowProgress(ProgressViewModel viewModel)
    {
        Dispatcher.UIThread.VerifyAccess();
        ProgressViewModels.Add(viewModel);
        if (ProgressViewModels.Count == 1)
            ShowDialog();
    }

    private async void ShowDialog()
    {
        await _dialogProvider.ShowDialogAsync(this);
    }

    public void HideProgress(ProgressViewModel viewModel)
    {
        Dispatcher.UIThread.VerifyAccess();
        ProgressViewModels.Remove(viewModel);
        if (ProgressViewModels.Count == 0)
            RaiseClose();
    }
}