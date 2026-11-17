using System.Collections.Specialized;
using Autofac;
using Avalonia.Threading;
using Bashka.Launcher;
using Bashka.Plugins.Core.Worker.ProgressUI;
using Bashka.Plugins.Core.Worker.Schedulers;
using Bashka.Tests.Helpers;

namespace Bashka.Tests.E2E;

[TestFixture]
public class WorkerProgressTests : BashkaAppTestBase
{
    [Test]
    public async Task ContinuationsAndNestedWorkShareTheirRootProgress()
    {
        await UITest.WaitNoText("Processing");
        var worker = MainWindow.Scope.Resolve<WorkerTaskFactory>();
        var presenter = MainWindow.Scope.Resolve<IProgressPresenter>();
        var resume = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var continued = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var finish = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var shownTitles = new List<string?>();
        NotifyCollectionChangedEventHandler recordRows = (_, args) =>
        {
            if (args.NewItems != null)
                foreach (ProgressViewModel row in args.NewItems)
                    shownTitles.Add(row.Title);
        };
        var first = Task.CompletedTask;
        var second = Task.CompletedTask;
        try
        {
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                presenter.ProgressViewModels.CollectionChanged += recordRows;
                first = worker.StartNewWorkerTask(async (_, _, ui) =>
                {
                    await resume.Task;
                    await ui.StartNew(() => { });
                    await worker.StartNewWorkerTask(async (_, _, nestedUi) => { await nestedUi.StartNew(() => { }); },
                        "Nested work");
                    continued.SetResult();
                    await finish.Task;
                }, "First worker");
                second = worker.StartNewWorkerTask(async (_, _, _) => await finish.Task, "Second worker");
            });

            resume.SetResult();
            await continued.Task.WaitAsync(TimeSpan.FromSeconds(20));
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                Assert.That(shownTitles, Is.EqualTo(new[] { "First worker", "Second worker" }));
                Assert.That(presenter.ProgressViewModels.Select(row => row.Title),
                    Is.EquivalentTo(shownTitles));
            });
        }
        finally
        {
            resume.TrySetResult();
            finish.TrySetResult();
            await Task.WhenAll(first, second).WaitAsync(TimeSpan.FromSeconds(20));
            await Dispatcher.UIThread.InvokeAsync(() =>
                presenter.ProgressViewModels.CollectionChanged -= recordRows);
        }

        await UITest.WaitNoText("Processing");
    }
}