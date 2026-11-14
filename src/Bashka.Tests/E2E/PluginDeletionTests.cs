using Autofac;
using Avalonia.Input;
using Avalonia.Threading;
using BaGetter.Protocol.Models;
using Bashka.Launcher;
using Bashka.Plugins.Core.Abstract.Data;
using Bashka.Plugins.Market;
using Bashka.Tests.Helpers;
using Consolonia.Core.Drawing.PixelBufferImplementation;
using Consolonia.NUnit;
using NuGet.Versioning;

namespace Bashka.Tests.E2E;

/// <summary>
/// Vibecoded
/// </summary>
[TestFixture]
public class PluginDeletionTests() : ConsoloniaAppTestBase<App>(new PixelBufferSize(140, 40))
{
    [Test]
    public async Task UninstallPluginsPreservesSharedPackages()
    {
        await UITest.WaitNoText("Processing");
        var catalog = new PluginsCatalog();
        string prefix = $"Bashka.DeletionTest.{Guid.NewGuid():N}";
        PluginEntry[] entries =
        [
            new(new SearchResult { PackageId = prefix + ".One", Version = "1.0.0" },
                PluginStatus.Installed, catalog, new NuGetVersion(1, 0, 0)),
            new(new SearchResult { PackageId = prefix + ".Two", Version = "2.0.0" },
                PluginStatus.Update, catalog, new NuGetVersion(1, 0, 0))
        ];
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
            var available = new PluginEntry(new SearchResult { PackageId = prefix, Version = "1.0.0" },
                PluginStatus.Available, catalog);
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

            await Dispatcher.UIThread.InvokeAsync(() => { execution = command.ExecuteAsync(context); });
            await UITest.WaitText("Confirm uninstall");
            await UITest.KeyInput(Key.Enter);
            await execution;
            Assert.That(directories.Any(Directory.Exists), Is.False);
            Assert.That(entries.All(entry => entry.IsGone), Is.True);
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
}
