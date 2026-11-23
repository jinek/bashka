using Autofac;
using Avalonia.Input;
using Avalonia.Threading;
using Bashka.Launcher;
using Bashka.Plugins.Core.Abstract;
using Bashka.Plugins.Core.Abstract.Data;
using Bashka.Plugins.Core.FunctionKeys;
using Bashka.Plugins.Foundation;
using Bashka.Plugins.Foundation.FileSystem;
using Bashka.Plugins.Foundation.ShellInteraction;
using Bashka.Tests.Helpers;

namespace Bashka.Tests.E2E;

[TestFixture]
public class ConnectionEditingTests : BashkaAppTestBase
{
    [Test]
    public async Task ConnectionsUseLocalFileListingAndExistingDeleteCommand()
    {
        await UITest.WaitNoText("Processing");
        string prefix = $"Bashka.DeleteConnection.{Guid.NewGuid():N}";
        string filePath = Path.Combine(ConnectionsCatalog.ConnectionsDirectory, prefix + " with 'quotes'.ini");
        string otherPath = Path.Combine(ConnectionsCatalog.ConnectionsDirectory, prefix + ".ini");
        string ignoredPath = Path.Combine(ConnectionsCatalog.ConnectionsDirectory, prefix + ".txt");
        string directoryPath = Path.Combine(ConnectionsCatalog.ConnectionsDirectory, prefix + ".folder.ini");
        var remote = new UnavailableRemoteConnection();
        using ILifetimeScope scope = MainWindow.Scope.BeginLifetimeScope(builder =>
            builder.RegisterInstance(remote).As<IShellConnection>());
        ConnectionsCatalog catalog = null!;
        var execution = Task.CompletedTask;
        try
        {
            Directory.CreateDirectory(ConnectionsCatalog.ConnectionsDirectory);
            Directory.CreateDirectory(directoryPath);
            SshConnectionSettings.CreateDefault(filePath);
            SshConnectionSettings.CreateDefault(otherPath);
            await File.WriteAllTextAsync(ignoredPath, "not a saved connection");
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                catalog = scope.Resolve<ConnectionsCatalog>();
                Assert.That(catalog.LifetimeScope, Is.SameAs(scope));
                Assert.That(catalog.ShellSession.ShellConnection, Is.TypeOf<LocalSystemShellConnection>());
                Assert.That(scope.Resolve<FileSystemCatalog>().ShellSession.ShellConnection, Is.SameAs(remote));
                Assert.That(scope.Resolve<ShellSession>().ShellConnection, Is.SameAs(remote));
                execution = catalog.NavigateAsync(string.Empty);
            });
            await execution.WaitAsync(TimeSpan.FromSeconds(20));
            Assert.That(catalog.CurrentPath, Is.EqualTo(ConnectionsCatalog.ConnectionsDirectory));
            Assert.That(catalog.ShellSession.ShellConnection, Is.TypeOf<LocalSystemShellConnection>());
            Assert.That(catalog.CanNavigateBack, Is.False);
            Assert.That(catalog.Children.OfType<NewConnectionEntry>().Count(), Is.EqualTo(1));
            Assert.That(catalog.Children.OfType<FileEntry>().Any(entry =>
                entry.FullPath == ignoredPath || entry.FullPath == directoryPath), Is.False);

            SwitchConnectionEntry entry = catalog.Children.OfType<SwitchConnectionEntry>()
                .Single(connection => connection.FullPath == filePath);
            var context = new CommandExecutionContext([entry], catalog, null, null, null);
            var delete = scope.Resolve<DeleteDirOrFileCommand>();
            Assert.That(delete, Is.InstanceOf<IFunctionBarCommand<ConnectionsCatalog>>());
            Assert.That(delete.CanExecute(context), Is.True);
            Assert.That(delete.CanExecute(context with { Entries = [new NewConnectionEntry(catalog)] }), Is.False);

            await Dispatcher.UIThread.InvokeAsync(() => { execution = ((ICommand)delete).ExecuteAsync(context); });
            await UITest.WaitText("Confirm delete");
            await UITest.KeyInput(Key.Escape);
            await execution.WaitAsync(TimeSpan.FromSeconds(20));
            Assert.That(File.Exists(filePath), Is.True);
            Assert.That(entry.IsGone, Is.False);

            await Dispatcher.UIThread.InvokeAsync(() => { execution = ((ICommand)delete).ExecuteAsync(context); });
            await UITest.WaitText("Confirm delete");
            await UITest.KeyInput(Key.Enter);
            await execution.WaitAsync(TimeSpan.FromSeconds(20));
            Assert.That(File.Exists(filePath), Is.False);
            Assert.That(entry.IsGone, Is.True);
            Assert.That(delete.CanExecute(context), Is.False);
            Assert.That(File.Exists(otherPath), Is.True);
            Assert.That(File.Exists(ignoredPath), Is.True);
            Assert.That(Directory.Exists(directoryPath), Is.True);
            await Dispatcher.UIThread.InvokeAsync(() => { execution = catalog.NavigateAsync(string.Empty); });
            await execution.WaitAsync(TimeSpan.FromSeconds(20));
            Assert.That(catalog.Children.OfType<FileEntry>().Any(item => item.FullPath == filePath), Is.False);
        }
        finally
        {
            File.Delete(filePath);
            File.Delete(otherPath);
            File.Delete(ignoredPath);
            Directory.Delete(directoryPath, true);
        }
    }

    [Test]
    public async Task CreatingConnectionPassesRealFileEntryToEditorAndRefreshesSettings()
    {
        await UITest.WaitNoText("Processing");
        string title = $"Connection with spaces {Guid.NewGuid():N}";
        string filePath = Path.Combine(ConnectionsCatalog.ConnectionsDirectory, title + ".ini");
        var editor = new RecordingEditor();
        using ILifetimeScope scope = MainWindow.Scope.BeginLifetimeScope(builder =>
            builder.RegisterInstance(editor).Keyed<ICommand>(ICommand.EditCommandKey));
        ConnectionsCatalog catalog = null!;
        var execution = Task.CompletedTask;
        try
        {
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                Assert.That(MainWindow.Scope.ResolveKeyed<ICommand>(ICommand.EditCommandKey),
                    Is.TypeOf<EditFileCommand>());
                catalog = scope.Resolve<ConnectionsCatalog>();
                var context = new CommandExecutionContext([], catalog, null, null, null);
                execution = scope.Resolve<NewConnectionCommand>().ExecuteAsync(context);
            });
            await UITest.WaitText("Enter connection name");
            await UITest.StringInput(title);
            await UITest.KeyInput(Key.Enter);
            await execution.WaitAsync(TimeSpan.FromSeconds(20));

            Assert.That(editor.Entry, Is.TypeOf<SwitchConnectionEntry>());
            Assert.That(editor.Entry, Is.InstanceOf<FileEntry>());
            Assert.That(editor.Entry!.FullPath, Is.EqualTo(filePath));
            Assert.That(editor.Entry.Path, Is.EqualTo(title + ".ini"));
            Assert.That(editor.Entry.ParentCatalog, Is.SameAs(catalog));
            var editContext = new CommandExecutionContext([editor.Entry], catalog, null, null, null);
            Assert.That(new EditFileCommand(new TextEditorSettings()).CanExecute(editContext), Is.True);
            Assert.That(new SwitchConnectionCommand().CanExecute(editContext), Is.True);
            SwitchConnectionEntry refreshed = catalog.Children.OfType<SwitchConnectionEntry>()
                .Single(entry => entry.FullPath == filePath);
            Assert.That(refreshed.Name, Is.EqualTo(title));
            Assert.That(refreshed.DisplayName, Is.EqualTo(title));
            Assert.That(refreshed.Extension, Is.EqualTo("example.test:2222"));
            Assert.That(new EditFileCommand(new TextEditorSettings()).CanExecute(editContext with
            {
                Entries = [new NewConnectionEntry(catalog)]
            }), Is.False);

            var local = scope.Resolve<LocalSystemConnectionCatalog>();
            await local.NavigateAsync(string.Empty);
            Assert.That(local.Children, Is.Empty, "local system is connected already");
            using ILifetimeScope remoteScope = scope.BeginLifetimeScope(builder =>
                builder.RegisterInstance(new UnavailableRemoteConnection()).As<IShellConnection>());
            local = remoteScope.Resolve<LocalSystemConnectionCatalog>();
            await local.NavigateAsync(string.Empty);
            Entry localEntry = local.Children.Single();
            var localContext = new CommandExecutionContext([localEntry], local, null, null, null);
            Assert.That(new SwitchConnectionCommand().CanExecute(localContext), Is.True);
            Assert.That(new EditFileCommand(new TextEditorSettings()).CanExecute(localContext), Is.False);
        }
        finally
        {
            File.Delete(filePath);
        }
    }

    private sealed class RecordingEditor : ICommand
    {
        public FileEntry? Entry { get; private set; }

        public bool CanExecute(CommandExecutionContext context)
        {
            return true;
        }

        public async Task ExecuteAsync(CommandExecutionContext context)
        {
            Entry = (FileEntry)context.Entries.Single();
            await File.WriteAllTextAsync(Entry.FullPath, """
                                                         [Connection]
                                                         Host=example.test
                                                         Port=2222
                                                         Username=test
                                                         Password=
                                                         """);
        }
    }

    private sealed class UnavailableRemoteConnection : IShellConnection
    {
        public string Title => "Remote connection must not be used for saved connection files";
        public byte MessageIdHitCount => 1;

        public IEnumerable<string> PrepareExecution(string command, string parameter)
        {
            throw new InvalidOperationException("Saved connection files must use the local shell.");
        }

        public bool IsEquivalentTo(object another)
        {
            return ReferenceEquals(this, another);
        }
    }
}