using Autofac;
using Avalonia.Input;
using Avalonia.Threading;
using Bashka.Launcher;
using Bashka.Plugins.Core.Abstract.Surrounding;
using Bashka.Plugins.Core.Worker.Schedulers;
using Bashka.Tests.Helpers;

namespace Bashka.Tests.E2E;

[TestFixture]
public class ExceptionHandlingTests : BashkaAppTestBase
{
    [Test]
    public async Task UserErrorFromUICallInsideWorkerIsShownOnce()
    {
        await UITest.WaitNoText("Processing");
        Task finished = ThrowFromUICallInsideWorker(new UserErrorException("Deep user error"));

        await UITest.WaitText("Deep user error");
        await UITest.KeyInput(Key.Escape);
        await finished.WaitAsync(TimeSpan.FromSeconds(20));
        await DrainDispatcher();

        await UITest.WaitNoText("Deep user error");
        await UITest.WaitNoText("Processing");
    }

    [Test]
    public async Task UserCancelFromUICallInsideWorkerIsSkipped()
    {
        await UITest.WaitNoText("Processing");
        Task finished = ThrowFromUICallInsideWorker(new UserCancelException());

        await finished.WaitAsync(TimeSpan.FromSeconds(20));
        await DrainDispatcher();

        await UITest.WaitNoText("Operation cancelled");
        await UITest.WaitNoText("Processing");
    }

    /// <summary>
    ///     Simulates a UI handler (async void, like commands) awaiting a worker which awaits a failing UI call.
    ///     Returned task completes right before the exception leaves the handler.
    /// </summary>
    private static Task ThrowFromUICallInsideWorker(Exception exception)
    {
        var worker = MainWindow.Scope.Resolve<WorkerTaskFactory>();
        var finished = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        Dispatcher.UIThread.Post(async () =>
        {
            try
            {
                await worker.StartNewWorkerTask(async (_, _, ui) => await ui.StartNew(() => throw exception),
                    "Failing work");
            }
            finally
            {
                finished.SetResult();
            }
        });
        return finished.Task;
    }

    /// <summary>
    ///     Lets pending dispatcher crossings and dialogs scheduled by them to happen
    /// </summary>
    private static async Task DrainDispatcher()
    {
        for (int i = 0; i < 3; i++)
            await Dispatcher.UIThread.InvokeAsync(() => { }, DispatcherPriority.Background);
    }
}
