using AsyncLocalRegion;
using Autofac.Features.AttributeFilters;
using Bashka.Plugins.Core.Worker.ProgressUI;

namespace Bashka.Plugins.Core.Worker.Schedulers;

public class WorkerTaskFactory(
    IProgressPresenter progressPresenter,
    [KeyFilter(TaskFactoryHelper.UITaskFactoryKey)]
    IUITaskFactory iuiTaskFactory)
    : TaskFactory(new WorkerTaskScheduler(progressPresenter, (TaskFactory)iuiTaskFactory))
{
    internal static readonly AsyncLocalParameter<Task> CurrentExecutionRootTask = new();

    internal static readonly AsyncLocalParameter<string?> Title = new();

    internal static ThreadLocal<CancellationToken> InternalCurrentCancellationToken { get; } = new();
    internal static ThreadLocal<IProgress<ProgressStatus>> InternalCurrentProgress { get; } = new();
    internal static ThreadLocal<TaskFactory> InternalCurrentUITaskFactory { get; } = new();
    public static CancellationToken CurrentCancellationToken => InternalCurrentCancellationToken.Value;
    public static IProgress<ProgressStatus> CurrentProgress => InternalCurrentProgress.Value!;
    public static TaskFactory CurrentUITaskFactory => InternalCurrentUITaskFactory.Value!;
}