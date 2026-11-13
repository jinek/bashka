using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Runtime.Versioning;
using BaGetter.Protocol.Models;
using Bashka.Plugins.Core.Abstract.Data;
using Bashka.Plugins.Core.Abstract.Surrounding;
using Bashka.Plugins.Core.Worker.Schedulers;
using NuGet.Frameworks;

namespace Bashka.Plugins.Market;

public class InstallPluginCommand(WorkerTaskFactory workerTaskFactory)
    : CatalogCommand<PluginsCatalog, PluginEntry>(null, false, true)
{
    public override async Task ExecuteAsync(CatalogCommandContext<PluginsCatalog, PluginEntry> context)
    {
        SearchResult package = context.SingleEntry.Package;
        ExceptionDispatchInfo? failure = null;
        await workerTaskFactory.StartNewWorkerTask((cancellationToken, progress, _) =>
            {
                cancellationToken.ThrowIfCancellationRequested();
                progress.ReportProgress("Preparing installation...");
                string directory = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "Bashka", "Plugins", package.PackageId);
                System.IO.Directory.CreateDirectory(directory);
                string framework = NuGetFramework.ParseFrameworkName(
                    typeof(InstallPluginCommand).Assembly.GetCustomAttribute<TargetFrameworkAttribute>()!.FrameworkName,
                    DefaultFrameworkNameProvider.Instance).GetShortFolderName();
                string dependenciesFile = Path.Combine(directory, "paket.dependencies");
                System.IO.File.WriteAllText(dependenciesFile, $"""
                                                               source https://api.nuget.org/v3/index.json
                                                               framework: {framework}
                                                               storage: packages

                                                               nuget {package.PackageId} = {package.Version}
                                                               """);
                cancellationToken.ThrowIfCancellationRequested();
                progress.ReportProgress($"Installing {package.PackageId} {package.Version}...");
                new Paket.Dependencies(dependenciesFile).Install(false);
                progress.ReportProgress("Installation complete", 100);

                return Task.CompletedTask;
            }, $"Installing {package.PackageId}");
    }
}