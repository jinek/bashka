using Bashka.Plugins.Core.Abstract;
using Bashka.Plugins.Core.Abstract.Data;
using Bashka.Plugins.Core.Abstract.Surrounding;
using Bashka.Plugins.Core.Scoping;
using Bashka.Plugins.Foundation;
using Bashka.Plugins.Foundation.FileSystem;
using Bashka.Plugins.Foundation.ShellInteraction;

namespace Bashka.Tests;

[TestFixture]
[NonParallelizable]
public class TextEditorTests
{
    [SetUp]
    public void SetUp()
    {
        _directory = Path.Combine(Path.GetTempPath(), $"Bashka.Editors.{Guid.NewGuid():N}");
        Directory.CreateDirectory(_directory);
    }

    [TearDown]
    public void TearDown()
    {
        Directory.Delete(_directory, true);
    }

    private string _directory = null!;

    [TestCase(true, "nano.exe,micro.exe,edit.exe,msedit.exe,edit.net.exe", "edit.net.exe")]
    [TestCase(true, "nano.exe,micro.exe,edit.exe,msedit.exe", "msedit.exe")]
    [TestCase(true, "nano.exe,micro.exe,edit.exe", "edit.exe")]
    [TestCase(true, "nano.exe,micro.exe", "micro.exe")]
    [TestCase(true, "nano.exe", "nano.exe")]
    [TestCase(false, "nano,msedit,micro,edit.net", "edit.net")]
    [TestCase(false, "nano,msedit,micro", "micro")]
    [TestCase(false, "nano,msedit", "msedit")]
    [TestCase(false, "nano", "nano")]
    public void ChoosesPreferredInstalledEditor(bool windows, string installed, string expected)
    {
        foreach (string name in installed.Split(',')) CreateExecutable(_directory, name);
        Assert.That(EditorLocator.FindEditor(TextEditorSettings.AutoEditor, windows, [_directory]),
            Is.EqualTo(Path.Combine(_directory, expected)));
    }

    [Test]
    public void PreferenceTakesPriorityOverPathOrderAndSkipsNonExecutableFiles()
    {
        if (OperatingSystem.IsWindows()) Assert.Ignore("This scenario checks Unix executable permissions.");
        string second = Path.Combine(_directory, "second");
        Directory.CreateDirectory(second);
        CreateExecutable(_directory, "nano");
        File.WriteAllText(Path.Combine(_directory, "edit.net"), string.Empty);
        CreateExecutable(second, "edit.net");
        Assert.That(EditorLocator.FindEditor("AUTO", false, [_directory, second]),
            Is.EqualTo(Path.Combine(second, "edit.net")));
    }

    [TestCase(false, "edit.net,micro,nano", "nano", "nano")]
    [TestCase(false, "edit.net,micro", "Edit.NET", "edit.net")]
    [TestCase(false, "edit.net", "micro", null)]
    [TestCase(true, "edit.net.exe,micro.exe", "micro", "micro.exe")]
    [TestCase(true, "edit.net.exe,micro.exe", "micro.exe", "micro.exe")]
    public void ConfiguredEditorIsLookedUpInPath(bool windows, string installed, string editor, string? expected)
    {
        foreach (string name in installed.Split(',')) CreateExecutable(_directory, name);
        Assert.That(EditorLocator.FindEditor(editor, windows, [_directory]),
            Is.EqualTo(expected == null ? null : Path.Combine(_directory, expected)));
    }

    [Test]
    public void ConfiguredFullPathIsUsedAsIs()
    {
        string other = Path.Combine(_directory, "other");
        Directory.CreateDirectory(other);
        CreateExecutable(other, "my-editor");
        string fullPath = Path.Combine(other, "my-editor");
        Assert.That(EditorLocator.FindEditor(fullPath, OperatingSystem.IsWindows(), [_directory]),
            Is.EqualTo(fullPath));
        Assert.That(EditorLocator.FindEditor(fullPath + "-missing", OperatingSystem.IsWindows(), [_directory]),
            Is.Null);
    }

    [Test]
    public async Task EditorCommandDiscoversEditorAndPassesFullFilePathToResidentExecutor()
    {
        string executable = "edit.net" + (OperatingSystem.IsWindows() ? ".exe" : string.Empty);
        CreateExecutable(_directory, executable);
        string? previousPath = Environment.GetEnvironmentVariable("PATH");
        try
        {
            Environment.SetEnvironmentVariable("PATH", _directory);
            ICommand command = new EditFileCommand(new TextEditorSettings());
            var catalog = new RecordingCatalog(_directory);
            var entry = new FileEntry
            {
                Path = "file with spaces.txt", Name = "file with spaces.txt",
                FullPath = Path.Combine(_directory, "file with spaces.txt"), ParentCatalog = catalog
            };
            var context = new CommandExecutionContext([entry], catalog, null, null, null);
            Assert.That(command.CanExecute(context), Is.True);
            await command.ExecuteAsync(context);
            Assert.That(catalog.Execution,
                Is.EqualTo((_directory, Path.Combine(_directory, executable), entry.FullPath)));

            string micro = "micro" + (OperatingSystem.IsWindows() ? ".exe" : string.Empty);
            CreateExecutable(_directory, micro);
            await new EditFileCommand(new TextEditorSettings { Editor = "micro" }).ExecuteAsync(context);
            Assert.That(catalog.Execution!.Value.Application, Is.EqualTo(Path.Combine(_directory, micro)),
                "configured editor wins over the preferred one");

            command = new EditFileCommand(new TextEditorSettings { Editor = "missing-editor" });
            Assert.That(Assert.ThrowsAsync<UserErrorException>(() => command.ExecuteAsync(context))!.Message,
                Does.Contain("missing-editor"), "configured editor does not fall back to detection");

            command = new EditFileCommand(new TextEditorSettings());
            File.Delete(Path.Combine(_directory, executable));
            File.Delete(Path.Combine(_directory, micro));
            Assert.ThrowsAsync<UserErrorException>(() => command.ExecuteAsync(context));
        }
        finally
        {
            Environment.SetEnvironmentVariable("PATH", previousPath);
        }
    }

    private static void CreateExecutable(string directory, string name)
    {
        string path = Path.Combine(directory, name);
        File.WriteAllText(path, string.Empty);
        if (!OperatingSystem.IsWindows())
            File.SetUnixFileMode(path, UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute);
    }

    private sealed class RecordingCatalog(string directory) : BaseObject, IPtyExecutor
    {
        public (string WorkingDirectory, string Application, string Arguments)? Execution { get; private set; }
        public string CurrentPath => directory;
        public string Title => "Editor test";
        public IEnumerable<Entry> Children => [];
        public bool CanNavigateBack => false;

        public Task<string> ResolveCurrentPathAsync()
        {
            return Task.FromResult(directory);
        }

        public Task NavigateAsync(string path)
        {
            throw new NotSupportedException();
        }

        public Task NavigateBack()
        {
            throw new NotSupportedException();
        }

        public Task ExecuteAsResidentAsync(string workingDir, string applicationName, string arguments,
            CancellationToken cancellationToken)
        {
            Execution = (workingDir, applicationName, arguments);
            return Task.CompletedTask;
        }
    }
}