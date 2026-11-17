using Avalonia.Input;
using Bashka.Tests.Helpers;

namespace Bashka.Tests.E2E;

[TestFixture]
public class BasicTests : BashkaAppTestBase
{
    private const string SandboxFolder = "E2ESandbox";
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

        // copy: make the right panel point to the same sandbox folder as destination
        await UITest.KeyInput(Key.Tab);
        await QuickSearchAndOpen(SandboxFolder);
        await UITest.KeyInput(Key.Tab);

        // copy: duplicate the sample file under a new name into the (now matching) other panel
        await QuickSearchAndSelect(SampleFile);
        await UITest.KeyInput(Key.F5);
        await UITest.KeyInput(Key.A, RawInputModifiers.Control);
        await UITest.StringInput(CopiedFileName);
        await UITest.KeyInput(Key.Enter);
        await UITest.WaitText(@"sample_copy +txt");

        // opening roots: the roots drop-down always contains the "All catalogs" entry
        await UITest.KeyInput(Key.F1, RawInputModifiers.Alt);
        await UITest.WaitText("All catalogs");
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