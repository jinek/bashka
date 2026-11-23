using System.Text.RegularExpressions;
using Autofac;
using Avalonia;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Threading;
using Bashka.Launcher;
using Bashka.Plugins.Core.Configuration;
using Bashka.Plugins.Core.Orthodox;
using Bashka.Plugins.Core.Orthodox.UI;
using Bashka.Plugins.Foundation.ShellInteraction;
using Bashka.Tests.Helpers;
using Consolonia.Core.Drawing.PixelBufferImplementation;

namespace Bashka.Tests.E2E;

[TestFixture]
public class BasicTests : BashkaAppTestBase
{
    private const string SandboxFolder = "E2ESandbox";
    private const string CurrentRowSandboxFolder = "CurrentRowSandbox";
    private const string SampleFile = "sample.txt";
    private const string SampleFileDisplay = "sample +txt";
    private const string NewFolderName = "NewFolder";
    private const string CopiedFileName = "sample_copy.txt";

    public string Buffer => UITest.PixelBuffer.PrintBuffer();

    [Test]
    public async Task CheckBasics()
    {
        await WaitProcessing();
        // navigate into the sandbox folder shipped as a test asset
        await QuickSearchAndOpen(SandboxFolder);
        await UITest.WaitText(SampleFileDisplay);
        await WaitState(state => state.Contains($"LeftPath = {SandboxPath}", StringComparison.Ordinal),
            "Location of the left panel is not saved to the state");

        // sorting: cycle through a couple of sort modes, the listing must survive
        await UITest.KeyInput(Key.F3, RawInputModifiers.Control);
        await UITest.WaitText(SampleFileDisplay);
        await UITest.KeyInput(Key.F6, RawInputModifiers.Control);
        await UITest.WaitText(SampleFileDisplay);

        // creating a directory
        await UITest.KeyInput(Key.F7);
        await UITest.StringInput(NewFolderName);
        await UITest.KeyInput(Key.Enter);
        await UITest.WaitText(NewFolderName);

        // navigating: enter the new (empty) directory and go back
        await QuickSearchAndOpen(NewFolderName);
        await UITest.WaitNoText(SampleFileDisplay);
        await QuickSearchAndOpen("..");
        await UITest.WaitText(SampleFileDisplay);

        // deleting the last marked entry: nothing is marked anymore, so the current row of the unfocused panel
        // is shown with marked color, unlike other unmarked rows
        await QuickSearchAndSelect(NewFolderName);
        await UITest.WaitBuffer(
            buffer => LeftPanelInfo(buffer).StartsWith($"║{NewFolderName} ", StringComparison.InvariantCulture),
            $"{NewFolderName} is not selected");
        await UITest.KeyInput(Key.Insert);
        await UITest.KeyInput(Key.Up);
        await UITest.KeyInput(Key.F8);
        await UITest.WaitText($"delete {NewFolderName}\\?");
        await UITest.KeyInput(Key.Enter);
        await UITest.WaitText("DELETED");
        await QuickSearchAndSelect(SampleFile);
        await UITest.KeyInput(Key.Tab);
        await UITest.WaitBuffer(buffer => LeftPanelRowOf(buffer, SampleFileDisplay) is { } current &&
                                          LeftPanelRowOf(buffer, @"^║\.\. ") is { } other &&
                                          Foreground(buffer, current) != Foreground(buffer, other),
            "Unfocused current row is not shown with marked color after the last marked entry was deleted");
        await UITest.KeyInput(Key.Tab);

        // only the focused panel highlights its current row, the other one keeps selection without highlighting
        await WaitHighlightedRows(1, 0);
        await UITest.KeyInput(Key.Tab);
        await WaitHighlightedRows(0, 1);

        // copy: make the right panel point to the same sandbox folder as destination
        await QuickSearchAndOpen(SandboxFolder);
        await UITest.KeyInput(Key.Tab);

        // copy: duplicate the sample file under a new name into the (now matching) other panel
        await QuickSearchAndSelect(SampleFile);
        await UITest.KeyInput(Key.F5);
        await UITest.KeyInput(Key.A, RawInputModifiers.Control);
        await UITest.StringInput(CopiedFileName);
        await UITest.KeyInput(Key.Enter);
        await UITest.WaitText(@"sample_copy +txt");

        // opening roots: the roots drop-down always contains the "All catalogs" entry,
        // connections are represented by the single entry, local system is connected already
        await UITest.KeyInput(Key.F1, RawInputModifiers.Alt);
        await UITest.WaitText("All catalogs");
        await UITest.WaitText(@"│ ssh +│");
        Assert.That(Buffer, Does.Not.Contain("Reconnect to local system"));
        await UITest.KeyInput(Key.Escape);
        await UITest.WaitNoText("All catalogs");

        // viewing: F3 opens the file viewer, its content must be read and displayed,
        // and Escape must close it again
        await QuickSearchAndSelect(SampleFile);
        await UITest.KeyInput(Key.F3);
        await UITest.WaitText("sample content used by BasicTests");
        await UITest.KeyInput(Key.Escape);
        await UITest.WaitNoText("sample content used by BasicTests");
    }

    [Test]
    public async Task PanelLocationIsRestored()
    {
        await WaitProcessing();
        await OpenLeftPanelAt(SandboxPath);
        await UITest.WaitBuffer(_ => Commander.LeftPanel!.CurrentCatalog?.CurrentPath == SandboxPath,
            "Left panel is not opened at the saved location");
        await UITest.WaitText(SampleFileDisplay);
        Assert.That(Commander.RightPanel!.CurrentCatalog?.CurrentPath,
            Is.EqualTo(TestContext.CurrentContext.TestDirectory), "location of the other panel is kept apart");
    }

    [Test]
    public async Task RootTitleIsCutInTheMiddle()
    {
        await WaitProcessing();
        string[] texts = await Dispatcher.UIThread.InvokeAsync(() =>
        {
            var title = new MiddleTrimmedTextBlock { FullText = "0123456789" };
            return new[] { 20d, 10, 6, 5, 1 }.Select(width =>
            {
                title.Measure(new Size(width, 1));
                return title.Text!;
            }).ToArray();
        });
        Assert.That(texts, Is.EqualTo(new[] { "0123456789", "0123456789", "01..89", "01..9", "." }));

        // long location in the panel header is cut, both corners of the panel stay
        string directory = Path.Combine(Path.GetTempPath(), "Bashka.LongTitle." + new string('x', 80));
        Directory.CreateDirectory(directory);
        try
        {
            await OpenLeftPanelAt(directory);
            await UITest.WaitText(@"╔═+ \S*\.\.x+ ↓═+╗╔");
        }
        finally
        {
            Directory.Delete(directory);
        }
    }

    [Test]
    public async Task CurrentRowForeground()
    {
        const ushort first = 5, second = 6, third = 7, fourth = 8;
        await WaitProcessing();
        await QuickSearchAndOpen(CurrentRowSandboxFolder);
        await UITest.WaitText("fourth");

        // marking two rows, moving the cursor away from them; the panel lost focus while other rows are marked:
        // current unmarked row looks as any other unmarked
        await UITest.KeyInput(Key.Down);
        await UITest.KeyInput(Key.Down, RawInputModifiers.Shift);
        await UITest.KeyInput(Key.Down, RawInputModifiers.Shift);
        await UITest.WaitText(@"║Selected 2: \d+ files?, \d+ folders?, ");
        await UITest.KeyInput(Key.Down);
        await UITest.KeyInput(Key.Tab);
        await UITest.WaitBuffer(buffer => Foreground(buffer, fourth) == Foreground(buffer, third) &&
                                          Foreground(buffer, fourth) != Foreground(buffer, first) &&
                                          Foreground(buffer, first) == Foreground(buffer, second),
            "Unfocused current unmarked row does not look as unmarked while there are marked rows");
        Color marked = await Dispatcher.UIThread.InvokeAsync(() => Foreground(UITest.PixelBuffer, first));

        // the current row is marked: it must look like marked
        await UITest.KeyInput(Key.Tab);
        await UITest.KeyInput(Key.Up);
        await UITest.KeyInput(Key.Up);
        await UITest.WaitBuffer(buffer => Foreground(buffer, second) == marked &&
                                          Foreground(buffer, second) != Foreground(buffer, third),
            "Focused current marked row does not look marked");

        // the panel lost focus: current marked row still looks marked
        await UITest.KeyInput(Key.Tab);
        await UITest.WaitBuffer(buffer => Foreground(buffer, second) == marked,
            "Unfocused current marked row does not look marked");

        // the panel lost focus, nothing is marked: current row is shown with marked color
        await UITest.KeyInput(Key.Tab);
        await UITest.KeyInput(Key.Subtract);
        await UITest.KeyInput(Key.Tab);
        await UITest.WaitBuffer(buffer => Foreground(buffer, second) == marked &&
                                          Foreground(buffer, first) == Foreground(buffer, fourth),
            "Unfocused current row is not shown with marked color while nothing is marked");

        // entering another catalog having no marks, while the previous one has
        await UITest.KeyInput(Key.Tab);
        await UITest.KeyInput(Key.Insert);
        await UITest.KeyInput(Key.Up);
        await UITest.KeyInput(Key.Enter);
        await UITest.WaitText(@"inside +txt");
        await UITest.KeyInput(Key.Tab);
        await UITest.WaitBuffer(buffer => Foreground(buffer, 4) == marked,
            "Unfocused current row is not shown with marked color after entering a catalog without marks");
    }

    private static OrthodoxCommander Commander => MainWindow.Scope.Resolve<OrthodoxCommander>();

    /// <summary>
    ///     Recreates the left panel as on start with <paramref name="location" /> saved in the state
    /// </summary>
    private static async Task OpenLeftPanelAt(string location)
    {
        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            MainWindow.Scope.Resolve<OrthodoxCommanderState>().LeftPath = location;
            Commander.RecreatePanel(Commander.LeftPanel, new LocalSystemShellConnection());
        });
    }

    private static string SandboxPath => Path.Combine(TestContext.CurrentContext.TestDirectory, SandboxFolder);

    private static async Task WaitState(Func<string, bool> condition, string description)
    {
        DateTime deadline = DateTime.UtcNow.AddSeconds(20);
        while (!(File.Exists(Settings.StateFilePath) && condition(await File.ReadAllTextAsync(Settings.StateFilePath))))
        {
            if (DateTime.UtcNow > deadline) Assert.Fail(description);
            await Task.Delay(200);
        }
    }

    private static ushort? LeftPanelRowOf(PixelBuffer buffer, string regex)
    {
        int row = Array.FindIndex(LeftPanelLines(buffer), line => Regex.IsMatch(line, regex));
        return row == -1 ? null : (ushort)row;
    }

    private static string LeftPanelInfo(PixelBuffer buffer)
    {
        return LeftPanelLines(buffer)[buffer.Height - 4];
    }

    private static string[] LeftPanelLines(PixelBuffer buffer)
    {
        return [.. buffer.PrintBuffer().Split('\n').Select(line => line[..Math.Min(line.Length, buffer.Width / 2)])];
    }

    private static Color Foreground(PixelBuffer buffer, ushort y)
    {
        return buffer[2, y].Foreground.Color;
    }

    private async Task WaitHighlightedRows(int left, int right)
    {
        await UITest.WaitBuffer(buffer =>
                HighlightedRows(buffer, 3) == left && HighlightedRows(buffer, (ushort)(buffer.Width / 2 + 3)) == right,
            $"Highlighted rows are not {left} in the left panel and {right} in the right panel");
    }

    private static int HighlightedRows(PixelBuffer buffer, ushort x)
    {
        Color[] column =
        [
            .. Enumerable.Range(4, buffer.Height - 10).Select(y => buffer[x, (ushort)y].Background.Color)
        ];
        Color rowBackground = column.GroupBy(color => color).MaxBy(group => group.Count())!.Key;
        return column.Count(color => color != rowBackground);
    }

    private async Task QuickSearchAndSelect(string pattern)
    {
        await UITest.KeyInput(Key.F, RawInputModifiers.Control);
        await UITest.StringInput(pattern);
        await UITest.KeyInput(Key.Escape);
    }

    private async Task QuickSearchAndOpen(string pattern)
    {
        await QuickSearchAndSelect(pattern);
        await UITest.KeyInput(Key.Enter);
    }

    private async Task WaitProcessing()
    {
        await UITest.WaitNoText("Processing");
    }
}