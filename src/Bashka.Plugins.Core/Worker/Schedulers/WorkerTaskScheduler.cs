using System.Collections.Concurrent;
using Autofac.Features.AttributeFilters;
using Avalonia.Threading;
using Bashka.Plugins.Core.Worker.ProgressUI;

namespace Bashka.Plugins.Core.Worker.Schedulers;

internal class WorkerTaskScheduler : TaskScheduler
{
    private readonly IProgressPresenter _progressPresenter;
    private readonly TaskFactory _uiTaskFactory;

    public WorkerTaskScheduler(IProgressPresenter progressPresenter, [KeyFilter(TaskFactoryHelper.UITaskFactoryKey)] TaskFactory uiTaskFactory)
    {
        _progressPresenter = progressPresenter;
        _uiTaskFactory = uiTaskFactory;
    }

    private readonly ConcurrentDictionary<Task, object?> _schedule = new();

    protected override IEnumerable<Task>? GetScheduledTasks()
    {
        return _schedule.Keys;
    }

    protected override void QueueTask(Task task)
    {
        var cancellationTokenSource = new CancellationTokenSource();
        var progress = new Progress<ProgressStatus>();
        TaskFactory uiTaskFactory = _uiTaskFactory;//todo: implement UI task factory

        ProgressViewModel? dialogContent = null;
        IDisposable? currentExecutionRootTaskRegion = null;

        bool subtask = !WorkerTaskFactory.Title.HasCurrentValue || WorkerTaskFactory.CurrentExecutionRootTask.HasCurrentValue;
        if (!subtask)
        {
            currentExecutionRootTaskRegion = WorkerTaskFactory.CurrentExecutionRootTask.StartRegion(task);
            string? titleCurrentValue = WorkerTaskFactory.Title.CurrentValue;

            // Must complete synchronously before QueueTask returns, so dialogContent is set
            // before we register the completion continuation below.
            // Elevate the dispatcher priority for this specific UI call so it isn't delayed
            // behind normal-priority UI work; other, unrelated uses of uiTaskFactory stay Normal.
            using (UITaskScheduler.Priority.StartRegion(DispatcherPriority.Send))
            {
                uiTaskFactory.StartNew(() =>
                {
                    dialogContent = new ProgressViewModel(titleCurrentValue,
                        cancellationTokenSource,
                        progress,
                        uiTaskFactory);
                    _progressPresenter.ShowProgress(dialogContent);
                }).Wait();
            }
        }
        _schedule.TryAdd(task, null);
        
        if (!subtask)
        {
            task.ContinueWith(_ =>
            {
                // Same reasoning as above: elevate priority just for this call so HideProgress
                // wins the dispatcher queue race against the caller's own post-await continuation.
                using (UITaskScheduler.Priority.StartRegion(DispatcherPriority.Send))
                {
                    return uiTaskFactory.StartNew(() =>
                    {
                        Dispatcher.UIThread.VerifyAccess();
                        _progressPresenter.HideProgress(dialogContent!);
                    });
                }
            }, CancellationToken.None, TaskContinuationOptions.ExecuteSynchronously, Default).Unwrap();//unwrap was proposed by Junie to finish this before than async method continuation at the caller level
        }

        ThreadPool.QueueUserWorkItem(async _ =>
        {
            try
            {
                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    //todo: this is workaround: content shown in popup is not attached to visual tree at first. //todo: add test to check if that was resolved
                },DispatcherPriority.Background);
                // todo: proper parameters //todo: what does this comment mean?
                WorkerTaskFactory.InternalCurrentCancellationToken.Value = cancellationTokenSource.Token;
                WorkerTaskFactory.InternalCurrentProgress.Value =
                    progress;
                WorkerTaskFactory.InternalCurrentUITaskFactory.Value = uiTaskFactory;
                // ReSharper disable once MethodSupportsCancellation

                if (!TryExecuteTask(task))
                    throw new NotImplementedException();
                _schedule.Remove(task, out _);
            }
            finally
            {
                WorkerTaskFactory.InternalCurrentCancellationToken.Value = null;
                WorkerTaskFactory.InternalCurrentProgress.Value = null;
                WorkerTaskFactory.InternalCurrentUITaskFactory.Value = null;
                cancellationTokenSource.Dispose();

                if (!subtask)
                {
                    currentExecutionRootTaskRegion!.Dispose();
                }
            }
        });
    }

    protected override bool TryExecuteTaskInline(Task task, bool taskWasPreviouslyQueued)
    {
        return false;
    }
}
