using Bashka.Plugins.Core.Worker.ProgressUI;

namespace Bashka.Plugins.Core.Worker.Schedulers;

public static class TaskFactoryHelper
{
    public const string UITaskFactoryKey = "IuiTaskFactory";

    public static async Task StartNewWorkerTask(this WorkerTaskFactory taskFactory,
        Func<CancellationToken, IProgress<ProgressStatus>, TaskFactory, Task> action, string? title = null)
    {
        using IDisposable titleRegion = WorkerTaskFactory.Title.StartRegion(title);

        Task workerTask = null!;
        Task task = workerTask;
        workerTask = new Task(() =>
        {
            using IDisposable? rootRegion = WorkerTaskFactory.CurrentExecutionRootTask.HasCurrentValue
                ? null
                : WorkerTaskFactory.CurrentExecutionRootTask.StartRegion(task);
            WorkerTaskFactory.CurrentProgress.ReportProgress("Working...");
            action(WorkerTaskFactory.CurrentCancellationToken, WorkerTaskFactory.CurrentProgress,
                WorkerTaskFactory.CurrentUITaskFactory).GetAwaiter().GetResult(); //todo: should call async here better?
        }, taskFactory.CancellationToken, taskFactory.CreationOptions);
        workerTask.Start(taskFactory.Scheduler!);
        await workerTask;
    }

    public static void ReportProgress(this IProgress<ProgressStatus> progress, string status, byte progressValue = 0)
    {
        progress.Report(new ProgressStatus(progressValue, status));
    }
}