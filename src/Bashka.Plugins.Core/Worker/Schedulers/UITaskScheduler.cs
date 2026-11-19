using AsyncLocalRegion;
using Avalonia.Threading;

namespace Bashka.Plugins.Core.Worker.Schedulers;

public class UITaskScheduler : TaskScheduler
{
    // Lets a caller (e.g. WorkerTaskScheduler) mark the ambient dispatcher priority to use for
    // work items it queues via this scheduler, without changing IUITaskFactory's public API.
    // Wrap a call with Priority.StartRegion(...) to elevate priority just for that call;
    // everything else keeps the default (Normal) priority.
    public static readonly AsyncLocalParameter<DispatcherPriority> Priority = new();

    private static bool IsCalledFromWorker => WorkerTaskFactory.CurrentExecutionRootTask.HasCurrentValue;

    protected override IEnumerable<Task> GetScheduledTasks()
    {
        throw new NotImplementedException();
    }

    protected override void QueueTask(Task task)
    {
        DispatcherPriority priority = Priority.HasCurrentValue ? Priority.CurrentValue : DispatcherPriority.Normal;
        bool calledFromWorker = IsCalledFromWorker;

        Dispatcher.UIThread.Post(() =>
        {
            if (task.IsCompleted)
                // The task may have already been executed via TryExecuteTaskInline
                return;

            if (!TryExecuteTask(task))
                throw new NotImplementedException();
            if (!calledFromWorker)
                RaiseFaultOnDispatcher(task);
        }, priority);
    }

    protected override bool TryExecuteTaskInline(Task task, bool taskWasPreviouslyQueued)
    {
        if (!Dispatcher.UIThread.CheckAccess()) return false;
        TryExecuteTask(task);
        if (!IsCalledFromWorker)
            RaiseFaultOnDispatcher(task);
        return true;
    }

    private static void RaiseFaultOnDispatcher(Task task)
    {
        if (!task.IsFaulted) return;

        AggregateException aggregateException = task.Exception!;
        Exception exception = aggregateException.InnerExceptions.Count == 1
            ? aggregateException.InnerExceptions[0]
            : aggregateException;
        Dispatcher.UIThread.Post(() => throw new UITaskFaultedException(exception));
    }
}