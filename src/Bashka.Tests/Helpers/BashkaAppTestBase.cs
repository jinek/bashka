using System.Reflection;
using Avalonia;
using Avalonia.Threading;
using Bashka.Launcher;
using Bashka.Plugins.Core.Configuration;
using Consolonia.Core.Drawing.PixelBufferImplementation;
using Consolonia.NUnit;
using NUnit.Framework.Interfaces;

namespace Bashka.Tests.Helpers;

[StopRenderTimerAfterTest]
public abstract class BashkaAppTestBase() : ConsoloniaAppTestBase<App>(new PixelBufferSize(140, 40))
{
    private object? _renderTimer;

    protected override AppBuilder CreateAppBuilder()
    {
        // panel locations are restored from the state, every test starts from the default ones
        if (File.Exists(Settings.StateFilePath)) File.Delete(Settings.StateFilePath);
        return base.CreateAppBuilder();
    }

    [SetUp]
    public async Task CaptureRenderTimer()
    {
        _renderTimer = await Dispatcher.UIThread.InvokeAsync(() =>
        {
            object locator = typeof(AvaloniaLocator).GetProperty("Current")!.GetValue(null)!;
            Type timerType = typeof(AvaloniaLocator).Assembly.GetType("Avalonia.Rendering.IRenderTimer", true)!;
            return locator.GetType().GetMethod("GetService", [typeof(Type)])!.Invoke(locator, [timerType])!;
        });
    }

    // Avalonia never removes compositors from the render loop, so the render timer thread of a finished app
    // keeps ticking and touches Dispatcher.UIThread, claiming it after the next test resets the dispatcher.
    // It is stopped after Consolonia's teardown, because closing windows waits for the render loop.
    private void StopRenderTimer()
    {
        if (_renderTimer is not { } timer) return;
        _renderTimer = null;

        PropertyInfo tick = timer.GetType().GetProperty("Tick")!;
        using var lastTickFinished = new ManualResetEventSlim();
        tick.SetValue(timer, (Action<TimeSpan>)(_ =>
        {
            tick.SetValue(timer, null);
            // ReSharper disable once AccessToDisposedClosure Let it fail if needed
            lastTickFinished.Set();
        }));
        if (!lastTickFinished.Wait(TimeSpan.FromSeconds(5)))
            throw new TimeoutException("Render timer of the finished app did not stop.");
    }

    /// <summary>
    ///     todo: Vibe-coded, remove this in some time
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    private sealed class StopRenderTimerAfterTestAttribute : Attribute, ITestAction
    {
        public ActionTargets Targets => ActionTargets.Test;

        public void BeforeTest(ITest test)
        {
        }

        public void AfterTest(ITest test)
        {
            (test.Fixture as BashkaAppTestBase)?.StopRenderTimer();
        }
    }
}