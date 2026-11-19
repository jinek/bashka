using Autofac;
using Avalonia.Input;
using Avalonia.Threading;
using Bashka.Launcher;
using Bashka.Plugins.Foundation.FileSystem;
using Bashka.Tests.Helpers;

namespace Bashka.Tests.E2E;
// pure vibe-code

[TestFixture]
public class ResidentTerminalTests : BashkaAppTestBase
{
    [Test]
    public async Task NanoEditsFileWithSpacesInsideDialogAndReturnsToPanels()
    {
        if (!File.Exists("/usr/bin/nano")) Assert.Ignore("nano is required for this scenario.");
        await UITest.WaitNoText("Processing");
        string directory = Path.Combine(Path.GetTempPath(), $"Bashka.Nano.{Guid.NewGuid():N}");
        Directory.CreateDirectory(directory);
        string file = Path.Combine(directory, "file with spaces.txt");
        var execution = Task.CompletedTask;
        try
        {
            await File.WriteAllTextAsync(file, "resident editor test\n");
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                var catalog = MainWindow.Scope.Resolve<FileSystemCatalog>();
                execution = catalog.ExecuteAsResidentAsync(directory, "nano", file, CancellationToken.None);
            });
            await UITest.WaitText("GNU nano");
            await UITest.WaitText("resident editor test");
            await UITest.StringInput("edited ");
            await UITest.KeyInput(Key.O, RawInputModifiers.Control);
            await UITest.KeyInput(Key.Enter);
            await UITest.KeyInput(Key.X, RawInputModifiers.Control);
            await execution.WaitAsync(TimeSpan.FromSeconds(20));
            Assert.That(await File.ReadAllTextAsync(file), Does.Contain("edited resident editor test"));
            Assert.That(Directory.GetFiles(directory), Is.EqualTo(new[] { file }));
            await UITest.WaitNoText("GNU nano");
            await UITest.KeyInput(Key.F1, RawInputModifiers.Alt);
            await UITest.WaitText("All catalogs");
            await UITest.KeyInput(Key.Escape);
        }
        finally
        {
            Directory.Delete(directory, true);
        }
    }

    [Test]
    public async Task CancellationClosesResidentDialogAndReturnsToPanels()
    {
        if (OperatingSystem.IsWindows()) Assert.Ignore("This scenario uses a POSIX shell.");
        await UITest.WaitNoText("Processing");
        string directory = Path.Combine(Path.GetTempPath(), $"Bashka.Resident.{Guid.NewGuid():N}");
        Directory.CreateDirectory(directory);
        string script = Path.Combine(directory, "resident script.sh");
        using var cancellation = new CancellationTokenSource();
        var execution = Task.CompletedTask;
        try
        {
            await File.WriteAllTextAsync(script, "printf 'Resident ready\\n'\nread answer\n");
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                var catalog = MainWindow.Scope.Resolve<FileSystemCatalog>();
                execution = catalog.ExecuteAsResidentAsync(directory, "/bin/sh", script, cancellation.Token);
            });
            await UITest.WaitText("Resident ready");
            await cancellation.CancelAsync();
            Assert.ThrowsAsync<OperationCanceledException>(async () =>
                await execution.WaitAsync(TimeSpan.FromSeconds(20)));
            await UITest.WaitNoText("Resident ready");
            await UITest.KeyInput(Key.F1, RawInputModifiers.Alt);
            await UITest.WaitText("All catalogs");
            await UITest.KeyInput(Key.Escape);
        }
        finally
        {
            await cancellation.CancelAsync();
            Directory.Delete(directory, true);
        }
    }
}