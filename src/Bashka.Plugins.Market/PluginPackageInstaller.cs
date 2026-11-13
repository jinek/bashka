using System.Collections.Concurrent;
using BaGetter.Protocol;
using Microsoft.FSharp.Collections;
using Microsoft.FSharp.Control;
using Microsoft.FSharp.Core;
using NuGet.Common;
using NuGet.Configuration;
using NuGet.Packaging;
using NuGet.Packaging.Core;
using NuGet.Packaging.Signing;
using NuGet.Versioning;
using Paket;
using static Paket.PackageResolver;
using PackageVersion = System.Tuple<Paket.SemVerInfo, Microsoft.FSharp.Collections.FSharpList<Paket.PackageSources.PackageSource>>;

namespace Bashka.Plugins.Market;

internal sealed class PluginPackageInstaller(NuGetClient client, IReadOnlySet<string> hostPackages)
{//todo: low this is fully vibe-coded file
    private readonly ConcurrentDictionary<string, Task<bool>> _pluginPackages = new(StringComparer.OrdinalIgnoreCase);

    public async Task InstallAsync(string dependenciesFile, CancellationToken cancellationToken,
        IProgress<Core.Worker.ProgressUI.ProgressStatus> progress)
    {
        string directory = Path.GetDirectoryName(dependenciesFile)!;
        DependenciesFile dependencies = DependenciesFile.ReadFromFile(dependenciesFile);
        DependenciesGroup group = dependencies.GetGroup(Constants.MainDependencyGroup);
        FSharpMap<Domain.PackageName, ResolvedPackage> model = Resolve(
            FSharpFunc<GetPackageVersionsParameters, FSharpAsync<IEnumerable<PackageVersion>>>.FromConverter(parameters =>
                FSharpAsync.AwaitTask(GetVersionsAsync(parameters))),
            FSharpFunc<ResolverStrategy, FSharpFunc<GetPackageVersionsParameters, FSharpList<PackageVersion>>>.FromConverter(_ =>
                FSharpFunc<GetPackageVersionsParameters, FSharpList<PackageVersion>>.FromConverter(_ => FSharpList<PackageVersion>.Empty)),
            FSharpFunc<GetPackageDetailsParameters, FSharpAsync<PackageDetails>>.FromConverter(parameters =>
                FSharpAsync.AwaitTask(GetDetailsAsync(parameters))),
            group.Name, group.Options.ResolverStrategyForDirectDependencies, group.Options.ResolverStrategyForTransitives,
            group.Options.Settings.FrameworkRestrictions, SetModule.OfSeq(group.Packages), UpdateMode.Install).GetModelOrFail();

        string packagesDirectory = MarketHelpers.PackagesDirectory;
        var paths = new VersionFolderPathResolver(packagesDirectory);
        var extraction = new PackageExtractionContext(PackageSaveMode.Defaultv3, XmlDocFileSaveMode.None,
            ClientPolicyContext.GetClientPolicy(Settings.LoadDefaultSettings(null), NullLogger.Instance), NullLogger.Instance);
        foreach (ResolvedPackage package in model.Values)
        {
            cancellationToken.ThrowIfCancellationRequested();
            progress.Report(new Core.Worker.ProgressUI.ProgressStatus(0,
                $"Installing {package.Name.Name} {package.Version}..."));
            var identity = new PackageIdentity(package.Name.Name, NuGetVersion.Parse(package.Version.ToString()));
            await PackageExtractor.InstallFromSourceAsync(package.Source.Url, identity,
                async destination =>
                {
                    await using Stream archive = await client.DownloadPackageAsync(identity.Id, identity.Version, cancellationToken);
                    await archive.CopyToAsync(destination, cancellationToken);
                }, paths, extraction, cancellationToken, Guid.Empty);
        }

        var lockGroup = new LockFileGroup(group.Name, group.Options, model, FSharpList<ModuleResolver.ResolvedSourceFile>.Empty);
        new LockFile(Path.Combine(directory, "paket.lock"), MapModule.OfSeq(new[] { Tuple.Create(group.Name, lockGroup) })).Save();
        
        return;

        async Task<IEnumerable<PackageVersion>> GetVersionsAsync(GetPackageVersionsParameters parameters) =>
            await RunAsync(Paket.NuGet.GetVersions(false, null, directory, parameters));

        async Task<PackageDetails> GetDetailsAsync(GetPackageDetailsParameters parameters)
        {
            cancellationToken.ThrowIfCancellationRequested();
            PackageDetails details = await RunAsync(Paket.NuGet.GetPackageDetails(null, directory, false, parameters));
            var included = new List<Tuple<Domain.PackageName, VersionRequirement, Requirements.FrameworkRestrictions>>();
            foreach (var dependency in details.DirectDependencies)
            {
                string id = dependency.Item1.Name;
                if (hostPackages.Contains(id)) continue;
                if (await _pluginPackages.GetOrAdd(id, IsPluginAsync)) continue;
                included.Add(dependency);
            }
            return new PackageDetails(details.Name, details.Source, details.DownloadLink, details.LicenseUrl,
                details.Unlisted, SetModule.OfSeq(included));
        }

        async Task<bool> IsPluginAsync(string id) =>
            (await client.GetPackageMetadataAsync(id, cancellationToken)).Any(package =>
                package.Tags?.Contains("bashka-plugin", StringComparer.OrdinalIgnoreCase) == true);

        Task<T> RunAsync<T>(FSharpAsync<T> action) =>
            FSharpAsync.StartAsTask(action, null, FSharpOption<CancellationToken>.Some(cancellationToken));
    }
}
