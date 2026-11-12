using Bashka.Plugins.Core.Scoping;

namespace Bashka.Plugins.Core.Worker.ProgressUI;

public class ProgressViewModel : BaseObject
{
    private readonly CancellationTokenSource _cancellationTokenSource;
    private readonly TaskFactory _uiTaskFactory;

    internal ProgressViewModel(string? title, CancellationTokenSource cancellationTokenSourceSource,
        Progress<ProgressStatus> progress, TaskFactory uiTaskFactory)
    {
        Title = title;
        _cancellationTokenSource = cancellationTokenSourceSource;
        _uiTaskFactory = uiTaskFactory;
        progress.ProgressChanged += (_, args) =>
        {
            _uiTaskFactory.StartNew(() =>
            {
                Progress = args.Progress;
                Status = args.Status;
                IsIndeterminate = args.Progress == 0;
            });
        };
    }

    public string? Title { get; }

    public byte Progress { get; private set; }
    public string Status { get; private set; } = string.Empty;
    public bool IsIndeterminate { get; private set; }
    public bool CancellationRequested => _cancellationTokenSource.IsCancellationRequested;

    public void Cancel()
    {
        _cancellationTokenSource.Cancel();
        _uiTaskFactory.StartNew(() => { OnPropertyChanged(nameof(CancellationRequested)); });
    }
}