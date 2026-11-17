using Autofac;
using Avalonia.Input;
using Avalonia.Threading;
using BaGetter.Protocol.Models;
using Bashka.Launcher;
using Bashka.Plugins.Core.Abstract.Data;
using Bashka.Plugins.Market;
using Bashka.Tests.Helpers;
using NuGet.Versioning;
using NuGet.Packaging;
using Bashka.Plugins.Core.Worker.Schedulers;

namespace Bashka.Tests.E2E;

/// <summary>
/// Vibecoded
/// </summary>
[TestFixture]
public class PluginDeletionTests : BashkaAppTestBase
{
    [Test]
    public async Task UninstallPluginsPreservesSharedPackages()
    {
        await UITest.WaitNoText("Processing");
        var catalog = new PluginsCatalog();
        string prefix = $"Bashka.DeletionTest.{Guid.NewGuid():N}";
        PluginEntry[] entries =
        [
            CreateEntry(catalog, prefix + ".One", "1.0.0"),
            CreateEntry(catalog, prefix + ".Two", "2.0.0", true)
        ];
        foreach (PluginEntry entry in entries)
            new PluginMetadata(new NuGetVersion(1, 0, 0), 123, DateTime.Today).ApplyTo(entry);
        string[] directories = entries.Select(entry => Path.Combine(MarketHelpers.PluginsDirectory, entry.Name)).ToArray();
        string cachedPackage = Path.Combine(MarketHelpers.PackagesDirectory, prefix);
        string otherPlugin = Path.Combine(MarketHelpers.PluginsDirectory, prefix + ".Other");
        try
        {
            foreach (string directory in directories.Append(otherPlugin))
            {
                Directory.CreateDirectory(directory);
                await File.WriteAllTextAsync(Path.Combine(directory, "paket.lock"), "installation metadata");
            }
            Directory.CreateDirectory(cachedPackage);
            string cachedFile = Path.Combine(cachedPackage, "shared.dll");
            await File.WriteAllTextAsync(cachedFile, "shared package contents");

            var command = MainWindow.Scope.Resolve<DeletePluginPackageCommand>();
            var context = new CommandExecutionContext(entries, catalog, null, null, null);
            var available = CreateEntry(catalog, prefix, "1.0.0");
            Assert.That(command.CanExecute(context), Is.True);
            Assert.That(command.CanExecute(context with { Entries = [available] }), Is.False);
            Assert.That(command.CanExecute(context with { Entries = [entries[0], available] }), Is.False);

            Task execution = Task.CompletedTask;
            await Dispatcher.UIThread.InvokeAsync(() => { execution = command.ExecuteAsync(context); });
            await UITest.WaitText("Confirm uninstall");
            await UITest.WaitText("Restart Bashka");
            await UITest.KeyInput(Key.Escape);
            await execution;
            Assert.That(directories.All(Directory.Exists), Is.True);
            Assert.That(entries.All(entry => !entry.IsGone), Is.True);
            Assert.That(entries.All(entry => entry.InstalledVersion == new NuGetVersion(1, 0, 0) && entry.Size == 123), Is.True);

            await Dispatcher.UIThread.InvokeAsync(() => { execution = command.ExecuteAsync(context); });
            await UITest.WaitText("Confirm uninstall");
            await UITest.KeyInput(Key.Enter);
            await execution;
            Assert.That(directories.Any(Directory.Exists), Is.False);
            Assert.That(entries[0].IsGone, Is.False);
            Assert.That(entries[1].IsGone, Is.True);
            Assert.That(entries.All(entry => entry.Status == PluginStatus.Available && entry.InstalledVersion == null), Is.True);
            Assert.That(entries.All(entry => entry.Size == -1), Is.True);
            Assert.That(entries[0].TimeStamp, Is.EqualTo(DateTime.MinValue));
            Assert.That(entries[0].Category, Is.EqualTo(EntryCategory.Executable));
            Assert.That(command.CanExecute(context with { Entries = [entries[0]] }), Is.False);
            Assert.That(command.CanExecute(context), Is.False);
            Assert.That(Directory.Exists(otherPlugin), Is.True);
            Assert.That(await File.ReadAllTextAsync(cachedFile), Is.EqualTo("shared package contents"));
        }
        finally
        {
            foreach (string directory in directories.Append(otherPlugin).Append(cachedPackage))
                if (Directory.Exists(directory)) Directory.Delete(directory, true);
        }
    }

    [Test]
    public async Task RefreshPluginMetadataTracksInstallationAndUpdateOnUIThread()
    {
        await UITest.WaitNoText("Processing");
        string id = $"Bashka.MetadataTest.{Guid.NewGuid():N}";
        string directory = Path.Combine(MarketHelpers.PluginsDirectory, id);
        string cache = Path.GetDirectoryName(new VersionFolderPathResolver(MarketHelpers.PackagesDirectory)
            .GetInstallPath(id, new NuGetVersion(1, 0, 0)))!;
        PluginEntry entry = CreateEntry(new PluginsCatalog(), id, "2.0.0");
        entry.IsMarked = true;
        var notifications = new HashSet<string?>();
        bool allNotificationsOnUI = true;
        entry.PropertyChanged += (_, args) =>
        {
            allNotificationsOnUI &= Dispatcher.UIThread.CheckAccess();
            notifications.Add(args.PropertyName);
        };
        var ui = new UITaskFactory();
        try
        {
            await Task.Run(() => PluginMetadata.RefreshAsync(entry, ui));
            Assert.That(entry.Status, Is.EqualTo(PluginStatus.Available));
            Assert.That(entry.Size, Is.EqualTo(-1));
            Assert.That(entry.Details, Is.EqualTo(new[] { "Version: 2.0.0" }));

            await InstallVersion("1.0.0", "first archive");
            Assert.That(entry.Status, Is.EqualTo(PluginStatus.Update));
            Assert.That(entry.Extension, Is.EqualTo("Update"));
            Assert.That(entry.InstalledVersion, Is.EqualTo(new NuGetVersion(1, 0, 0)));
            Assert.That(entry.Details, Is.EqualTo(new[] { "Version: 2.0.0", "Installed: 1.0.0" }));
            Assert.That(entry.Size, Is.EqualTo(13));
            Assert.That(entry.Category, Is.EqualTo(EntryCategory.Executable));

            notifications.Clear();
            await InstallVersion("2.0.0", "updated archive contents");
            Assert.That(entry.Status, Is.EqualTo(PluginStatus.Installed));
            Assert.That(entry.Extension, Is.EqualTo("Installed"));
            Assert.That(entry.InstalledVersion, Is.EqualTo(new NuGetVersion(2, 0, 0)));
            Assert.That(entry.Size, Is.EqualTo(24));
            Assert.That(entry.Category, Is.EqualTo(EntryCategory.Default));
            Assert.That(notifications, Is.SupersetOf(new[] { "Status", "Extension", "InstalledVersion", "Details", "Size", "TimeStamp", "Category", "SelfHack" }));
            Assert.That(entry.Details, Is.EqualTo(new[] { "Version: 2.0.0", "Installed: 2.0.0" }));
            Assert.That(entry.FormatMetadata(), Does.Contain("Installed version: 2.0.0"));

            Directory.Delete(directory, true);
            await Task.Run(() => PluginMetadata.RefreshAsync(entry, ui));
            Assert.That(entry.Status, Is.EqualTo(PluginStatus.Available));
            Assert.That(entry.Extension, Is.Empty);
            Assert.That(entry.InstalledVersion, Is.Null);
            Assert.That(entry.Details, Is.EqualTo(new[] { "Version: 2.0.0" }));
            Assert.That(entry.Size, Is.EqualTo(-1));
            Assert.That(entry.TimeStamp, Is.EqualTo(DateTime.MinValue));
            Assert.That(entry.Category, Is.EqualTo(EntryCategory.Executable));
            Assert.That(entry.IsGone, Is.False);
            Assert.That(entry.IsMarked, Is.True);
            Assert.That(allNotificationsOnUI, Is.True);
        }
        finally
        {
            foreach (string path in new[] { directory, cache })
                if (Directory.Exists(path)) Directory.Delete(path, true);
        }

        async Task InstallVersion(string version, string contents)
        {
            Directory.CreateDirectory(directory);
            await File.WriteAllTextAsync(Path.Combine(directory, "paket.lock"), $"""
                NUGET
                  remote: https://api.nuget.org/v3/index.json
                    {id} ({version})
                """);
            var resolver = new VersionFolderPathResolver(MarketHelpers.PackagesDirectory);
            string archive = resolver.GetPackageFilePath(id, NuGetVersion.Parse(version));
            Directory.CreateDirectory(Path.GetDirectoryName(archive)!);
            await File.WriteAllTextAsync(archive, contents);
            File.SetLastWriteTime(archive, DateTime.Today.AddDays(-int.Parse(version[..1])));
            await Task.Run(() => PluginMetadata.RefreshAsync(entry, ui));
            Assert.That(entry.TimeStamp, Is.EqualTo(File.GetLastWriteTime(archive)));
        }
    }

    [Test]
    public async Task PartialUninstallKeepsCompletedEntriesRefreshed()
    {
        await UITest.WaitNoText("Processing");
        string prefix = $"Bashka.PartialDeletionTest.{Guid.NewGuid():N}";
        var catalog = new PluginsCatalog();
        PluginEntry[] entries =
        [
            CreateEntry(catalog, prefix + ".One", "1.0.0"),
            CreateEntry(catalog, prefix + ".Missing", "1.0.0")
        ];
        foreach (PluginEntry entry in entries)
            new PluginMetadata(new NuGetVersion(1, 0, 0), 123, DateTime.Today).ApplyTo(entry);
        string directory = Path.Combine(MarketHelpers.PluginsDirectory, entries[0].Name);
        try
        {
            Directory.CreateDirectory(directory);
            var command = MainWindow.Scope.Resolve<DeletePluginPackageCommand>();
            var context = new CommandExecutionContext(entries, catalog, null, null, null);
            Task execution = Task.CompletedTask;
            await Dispatcher.UIThread.InvokeAsync(() => { execution = command.ExecuteAsync(context); });
            await UITest.WaitText("Confirm uninstall");
            await UITest.KeyInput(Key.Enter);
            Assert.CatchAsync(async () => await execution);
            Assert.That(Directory.Exists(directory), Is.False);
            Assert.That(entries[0].Status, Is.EqualTo(PluginStatus.Available));
            Assert.That(entries[0].InstalledVersion, Is.Null);
            Assert.That(entries[0].Size, Is.EqualTo(-1));
            Assert.That(entries[1].Status, Is.EqualTo(PluginStatus.Installed));
            Assert.That(entries[1].InstalledVersion, Is.EqualTo(new NuGetVersion(1, 0, 0)));
            Assert.That(entries[1].Size, Is.EqualTo(123));
        }
        finally
        {
            if (Directory.Exists(directory)) Directory.Delete(directory, true);
        }
    }

    private static PluginEntry CreateEntry(PluginsCatalog catalog, string id, string version, bool localOnly = false) => new()
    {
        Path = id, Name = id, FullPath = "plugins/" + id, ParentCatalog = catalog,
        Package = new SearchResult { PackageId = id, Version = version }, LocalOnly = localOnly
    };

}
