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
        Assert.That(EditorLocator.FindEditor(windows, [_directory]),
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
        Assert.That(EditorLocator.FindEditor(false, [_directory, second]),
            Is.EqualTo(Path.Combine(second, "edit.net")));
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
            ICommand command = new EditFileCommand();
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

            File.Delete(Path.Combine(_directory, executable));
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