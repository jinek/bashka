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
        SynchronizationContext? firstContext = await probe.RunAppAsync();
        Assert.That(firstContext, Is.InstanceOf<AvaloniaSynchronizationContext>());

        await probe.RunAppAsync();

        Assert.That(probe.ContextBeforeWorkaround, Is.SameAs(firstContext),
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

        public async Task<SynchronizationContext?> RunAppAsync()
        {
            await SetUpApp();
            try
            {
                await UITest.WaitNoText("Processing");
                return await Dispatcher.UIThread.InvokeAsync(() => SynchronizationContext.Current);
            }
            finally
            {
                await TearDownApp();
            }
        }
    }
}
