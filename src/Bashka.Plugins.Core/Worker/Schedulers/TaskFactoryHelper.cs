using Bashka.Plugins.Core.Worker.ProgressUI;

namespace Bashka.Plugins.Core.Worker.Schedulers;

public static class TaskFactoryHelper
{
    public const string UITaskFactoryKey = "IuiTaskFactory";

    public static Task StartNewWorkerTask(this WorkerTaskFactory taskFactory,
        Func<CancellationToken, IProgress<ProgressStatus>, TaskFactory, Task> action, string? title = null)
    {
        IDisposable titleRegion = WorkerTaskFactory.Title.StartRegion(title);

        return taskFactory.StartNew(() =>
        {
            WorkerTaskFactory.CurrentProgress.ReportProgress("Working...");
            action(WorkerTaskFactory.CurrentCancellationToken, WorkerTaskFactory.CurrentProgress,
                WorkerTaskFactory.CurrentUITaskFactory).Wait();//todo: should call async here better?
        }).ContinueWith(_ => titleRegion.Dispose());
    }

    public static void ReportProgress(this IProgress<ProgressStatus> progress, string status, byte progressValue = 0)
    {
        progress.Report(new ProgressStatus(progressValue, status));
    }
}