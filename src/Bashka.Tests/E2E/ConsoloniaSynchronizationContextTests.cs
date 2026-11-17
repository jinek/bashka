using Avalonia;
using Avalonia.Threading;
using Bashka.Tests.Helpers;

namespace Bashka.Tests.E2E;

[TestFixture]
[NonParallelizable]
public class ConsoloniaSynchronizationContextTests
{
    [Test]
    public async Task ConsoloniaStillRetainsPreviousAppsSynchronizationContext()
    {
        var probe = new AppLifecycleProbe();
        await probe.RunAppAsync();
        bool previousContextCanResume = await probe.RunAppAsync();

        Assert.That(previousContextCanResume, Is.False,
            "Consolonia.NUnit no longer retains the previous app's synchronization context. " +
            "Remove the context reset in BashkaAppTestBase and this workaround reminder test.");
    }

    private sealed class AppLifecycleProbe : BashkaAppTestBase
    {
        public SynchronizationContext? ContextBeforeWorkaround { get; private set; }

        protected override AppBuilder CreateAppBuilder()
        {
            ContextBeforeWorkaround = SynchronizationContext.Current;
            return base.CreateAppBuilder();
        }

        public async Task<bool> RunAppAsync()
        {
            await SetUpApp();
            try
            {
                await UITest.WaitNoText("Processing");
                if (ContextBeforeWorkaround is not AvaloniaSynchronizationContext context)
                    return true;

                var resumed = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
                context.Post(_ => resumed.TrySetResult(), null);
                await Task.WhenAny(resumed.Task, Task.Delay(TimeSpan.FromSeconds(1)));
                return resumed.Task.IsCompleted;
            }
            finally
            {
                await TearDownApp();
            }
        }
    }
}