using Autofac;
using Avalonia.Input;
using Avalonia.Threading;
using Bashka.Launcher;
using Bashka.Plugins.Core.Abstract;
using Bashka.Plugins.Core.Abstract.Data;
using Bashka.Plugins.Core.Orthodox;
using Bashka.Plugins.Foundation;
using Bashka.Plugins.Foundation.FileSystem;
using Bashka.Plugins.Foundation.ShellInteraction;
using Bashka.Tests.Helpers;

namespace Bashka.Tests.E2E;

[TestFixture]
public class ConnectionEditingTests : BashkaAppTestBase
{
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
        Task execution = Task.CompletedTask;
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
            Assert.That(new EditFileCommand().CanExecute(editContext), Is.True);
            Assert.That(new SwitchConnectionCommand().CanExecute(editContext), Is.True);
            SwitchConnectionEntry refreshed = catalog.Children.OfType<SwitchConnectionEntry>()
                .Single(entry => entry.FullPath == filePath);
            Assert.That(refreshed.Name, Is.EqualTo(title));
            Assert.That(refreshed.DisplayName, Is.EqualTo(title));
            Assert.That(refreshed.Extension, Is.EqualTo("example.test:2222"));
            Assert.That(new EditFileCommand().CanExecute(editContext with
                { Entries = [new NewConnectionEntry(catalog)] }), Is.False);

            var local = scope.Resolve<LocalSystemConnectionCatalog>();
            await local.NavigateAsync(string.Empty);
            Entry localEntry = local.Children.Single();
            var localContext = new CommandExecutionContext([localEntry], local, null, null, null);
            Assert.That(new SwitchConnectionCommand().CanExecute(localContext), Is.True);
            Assert.That(new EditFileCommand().CanExecute(localContext), Is.False);
        }
        finally
        {
            File.Delete(filePath);
        }
    }

    private sealed class RecordingEditor : ICommand
    {
        public FileEntry? Entry { get; private set; }

        public bool CanExecute(CommandExecutionContext context) => true;

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
}
