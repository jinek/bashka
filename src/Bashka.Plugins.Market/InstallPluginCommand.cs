using System.Reflection;
using System.Text.Json;
using BaGetter.Protocol;
using BaGetter.Protocol.Models;
using Bashka.Plugins.Core.Abstract.Data;
using Bashka.Plugins.Core.Abstract.Surrounding;
using Bashka.Plugins.Core.Worker.Schedulers;

namespace Bashka.Plugins.Market;

public class InstallPluginCommand(WorkerTaskFactory workerTaskFactory)
    : CatalogCommand<PluginsCatalog, PluginEntry>(null, false, true)
{
    public override async Task ExecuteAsync(CatalogCommandContext<PluginsCatalog, PluginEntry> context)
    {
        SearchResult package = context.SingleEntry.Package;
        await workerTaskFactory.StartNewWorkerTask(async (cancellationToken, progress, uiTaskFactory) =>
            {
                cancellationToken.ThrowIfCancellationRequested();
                progress.ReportProgress("Preparing installation...");
                string appDataDir = Path.Combine(MarketHelpers.PluginsDirectory, package.PackageId);
                
                System.IO.Directory.CreateDirectory(appDataDir);
                string framework = MarketHelpers.GetFramework().GetShortFolderName();
                
                string dependenciesFile = Path.Combine(appDataDir, "paket.dependencies");
                await System.IO.File.WriteAllTextAsync(dependenciesFile, $"""
                                                                          source https://api.nuget.org/v3/index.json
                                                                          framework: {framework}
                                                                          storage: none

                                                                          nuget {package.PackageId} = {package.Version}
                                                                          """, cancellationToken);
                cancellationToken.ThrowIfCancellationRequested();
                progress.ReportProgress($"Installing {package.PackageId} {package.Version}...");
                string hostDependenciesFile = Path.Combine(AppContext.BaseDirectory,
                    $"{Assembly.GetEntryAssembly()!.GetName().Name}.deps.json");
                using JsonDocument hostDependencies =
                    JsonDocument.Parse(await System.IO.File.ReadAllTextAsync(hostDependenciesFile, cancellationToken));
                
                HashSet<string> hostPackages = hostDependencies.RootElement
                    .GetProperty("libraries")
                    .EnumerateObject()
                    .Select(library => Path.GetDirectoryName(library.Name)!)
                    .ToHashSet(StringComparer.OrdinalIgnoreCase);
                
                var installer = new PluginPackageInstaller(new NuGetClient("https://api.nuget.org/v3/index.json"),
                    hostPackages);
                
                await installer.InstallAsync(dependenciesFile, cancellationToken, progress);
                
                await PluginMetadata.RefreshAsync(context.SingleEntry, uiTaskFactory);
                progress.ReportProgress("Installation complete", 100);
            }, $"Installing {package.PackageId}");

    }
}
