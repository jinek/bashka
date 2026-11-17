using System.Reflection;
using Autofac;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Bashka.Launcher;
using Bashka.Plugins.Core.Dialog.Abstract;
using Bashka.Plugins.Foundation;
using Bashka.Tests.Helpers;
using Iciclecreek.Terminal;

namespace Bashka.Tests.E2E;

[TestFixture]
[NonParallelizable]
public class TerminalRenderingWorkaroundTests : BashkaAppTestBase
{
    private const string ProbeText = "Terminal glyph probe: Abc XYZ 0123456789";

    [Test]
    public async Task TerminalGlyphOptimizationStillCorruptsConsoloniaText()
    {
        await UITest.WaitNoText("Processing");
        string workingRendering = await RenderProbe();
        Assert.That(workingRendering, Does.Contain(ProbeText),
            "The control scenario must render correctly with the workaround enabled.");

        FieldInfo? fastPath = typeof(TerminalView).GetField("GlyphRunFastPathEnabled",
            BindingFlags.NonPublic | BindingFlags.Static);
        Assert.That(fastPath, Is.Not.Null,
            "The terminal library changed. Reassess the reflection workaround in TerminalDialogBehavior.");
        object? previousValue = fastPath!.GetValue(null);
        try
        {
            await Dispatcher.UIThread.InvokeAsync(() => fastPath.SetValue(null, true));
            string optimizedRendering = await RenderProbe();
            Assert.That(optimizedRendering, Does.Not.Contain(ProbeText),
                "The terminal optimization now renders correctly in Consolonia. " +
                "Remove the GlyphRunFastPathEnabled workaround from TerminalDialogBehavior and this reminder test.");
        }
        finally
        {
            await Dispatcher.UIThread.InvokeAsync(() => fastPath.SetValue(null, previousValue));
        }
    }

    private async Task<string> RenderProbe()
    {
        TerminalDialogContent dialog = null!;
        Task<object?> showDialog = null!;
        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            dialog = new TerminalDialogContent(Environment.CurrentDirectory, string.Empty, string.Empty);
            showDialog = MainWindow.Scope.Resolve<IDialogProvider>().ShowDialogAsync(dialog);
        });
        try
        {
            await UITest.WaitRendered();
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                var window = ((Consolonia.ConsoloniaLifetime)Avalonia.Application.Current!.ApplicationLifetime!).MainWindow!;
                TerminalControl terminal = window.GetVisualDescendants().OfType<TerminalControl>()
                    .Single(control => ReferenceEquals(control.DataContext, dialog));
                terminal.Terminal.Write(ProbeText);
                terminal.GetVisualDescendants().OfType<TerminalView>().Single().InvalidateVisual();
                Assert.That(terminal.Terminal.GetLine(0), Is.EqualTo(ProbeText),
                    "The emulator buffer must contain the original text before checking its rendering.");
            });
            await UITest.WaitRendered();
            return await Dispatcher.UIThread.InvokeAsync(() => UITest.PixelBuffer.PrintBuffer());
        }
        finally
        {
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                dialog.Close();
            });
            await showDialog;
        }
    }
}
