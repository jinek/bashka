using Autofac;
using Avalonia.Input;
using Avalonia.Threading;
using Bashka.Launcher;
using Bashka.Plugins.Core.Abstract;
using Bashka.Plugins.Core.Abstract.Data;
using Bashka.Plugins.Core.Configuration;
using Bashka.Plugins.Foundation.Configuration;
using Bashka.Plugins.Foundation.FileSystem;
using Bashka.Tests.Helpers;

namespace Bashka.Tests.E2E;

[TestFixture]
public class SettingsEditingTests : BashkaAppTestBase
{
    [Test]
    public async Task CatalogListsReadOnlyPluginFilesThenUserFilesThenState()
    {
        await UITest.WaitNoText("Processing");
        TestDataDirectory.DeleteSettings();
        Directory.CreateDirectory(Settings.UserDirectory);
        await File.WriteAllTextAsync(Path.Combine(Settings.UserDirectory, "mine.ini"),
            "[TextEditor]\nEditor = micro\n");
        await File.WriteAllTextAsync(Settings.StateFilePath, "[Panels]\nLeftPath = /\n");
        var editor = new RecordingEditor();
        using ILifetimeScope scope = MainWindow.Scope.BeginLifetimeScope(builder =>
            builder.RegisterInstance(editor).Keyed<ICommand>(ICommand.EditCommandKey));
        try
        {
            SettingsCatalog catalog = null!;
            var execution = Task.CompletedTask;
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                catalog = scope.Resolve<SettingsCatalog>();
                execution = catalog.NavigateAsync(string.Empty);
            });
            await execution.WaitAsync(TimeSpan.FromSeconds(20));

            Assert.That(catalog.CurrentPath, Is.EqualTo(Settings.UserDirectory));
            Entry[] entries = [.. catalog.Children];
            Assert.That(entries.Select(entry => entry.DisplayName), Is.EqualTo(new[]
            {
                "orthodoxcommander (Bashka.Plugins.Core)", "texteditor (Bashka.Plugins.Foundation)", "mine (User)",
                "state (State)"
            }));
            Assert.That(entries.Select(entry => entry.Extension), Is.All.EqualTo("ini"));
            Assert.That(entries.Select(entry => entry.Category), Is.EqualTo(new[]
            {
                EntryCategory.Hidden, EntryCategory.Hidden, EntryCategory.Default, EntryCategory.Important
            }));

            var delete = scope.Resolve<DeleteDirOrFileCommand>();
            var editPluginSettings = scope.Resolve<EditPluginSettingsCommand>();
            Assert.That(delete.CanExecute(Context(entries[0])), Is.False, "plugin files are read-only");
            Assert.That(delete.CanExecute(Context(entries[2])), Is.True);
            Assert.That(delete.CanExecute(Context(entries[3])), Is.True);
            Assert.That(editPluginSettings.CanExecute(Context(entries[0])), Is.True);
            Assert.That(editPluginSettings.CanExecute(Context(entries[2])), Is.False);

            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                execution = editPluginSettings.ExecuteAsync(Context(entries[0]));
            });
            await execution.WaitAsync(TimeSpan.FromSeconds(20));
            Assert.That(editor.TextBeforeEdit, Does.StartWith("; Read-only defaults of Bashka.Plugins.Core"));
            Assert.That(editor.TextBeforeEdit, Does.Contain("QuickSearchCommand = CTRL+F"));
            Assert.That(File.Exists(editor.Entry!.FullPath), Is.False, "temporary copy is discarded");
        }
        finally
        {
            TestDataDirectory.DeleteSettings();
        }

        CommandExecutionContext Context(Entry entry)
        {
            return new CommandExecutionContext([entry], entry.ParentCatalog, null, null, null);
        }
    }

    [Test]
    public async Task SettingsAreOpenedFromOptionsMenuInsteadOfRoots()
    {
        await UITest.WaitNoText("Processing");
        await UITest.KeyInput(Key.F1, RawInputModifiers.Alt);
        await UITest.WaitText("All catalogs");
        await UITest.WaitNoText("orthodoxcommander");
        await UITest.KeyInput(Key.Escape);
        await UITest.WaitNoText("All catalogs");

        await UITest.KeyInput(Key.F9);
        await UITest.KeyInput(3, Key.Right);
        await UITest.KeyInput(Key.Enter);
        await UITest.WaitText("Settings");
        await UITest.KeyInput(Key.Enter);
        await UITest.WaitText(@"orthodoxcommander \(Bashka\.Plugins\.Core\)");
        await UITest.WaitText(@"texteditor \(Bashka\.Plugins\.Foundation\)");

        // cloning defaults of a plugin into a user settings file
        TestDataDirectory.DeleteSettings();
        try
        {
            await UITest.KeyInput(Key.F, RawInputModifiers.Control);
            await UITest.StringInput("orthodoxcommander");
            await UITest.KeyInput(Key.Escape);
            await UITest.WaitText("Clone");
            await UITest.KeyInput(Key.F6);
            await UITest.WaitText("Enter user settings file name");
            await UITest.KeyInput(Key.A, RawInputModifiers.Control);
            await UITest.StringInput("mine");
            await UITest.KeyInput(Key.Enter);
            await UITest.WaitText(@"mine \(User\)");
            Assert.That(await File.ReadAllTextAsync(Path.Combine(Settings.UserDirectory, "mine.ini")),
                Does.Contain("QuickSearchCommand = CTRL+F"));
        }
        finally
        {
            TestDataDirectory.DeleteSettings();
        }
    }

    private sealed class RecordingEditor : ICommand
    {
        public FileEntry? Entry { get; private set; }
        public string TextBeforeEdit { get; private set; } = string.Empty;

        public bool CanExecute(CommandExecutionContext context)
        {
            return true;
        }

        public async Task ExecuteAsync(CommandExecutionContext context)
        {
            Entry = (FileEntry)context.Entries.Single();
            TextBeforeEdit = await File.ReadAllTextAsync(Entry.FullPath);
        }
    }
}