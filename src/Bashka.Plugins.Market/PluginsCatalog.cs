using BaGetter.Protocol;
using BaGetter.Protocol.Models;
using Bashka.Plugins.Core.Abstract.Data;
using Bashka.Plugins.Core.Abstract.Surrounding;
using Bashka.Plugins.Foundation.FileSystem;
using NuGet.Packaging;
using NuGet.Versioning;
using Paket;

namespace Bashka.Plugins.Market;

public sealed class PluginsCatalog : PathCatalog, IReadStreamCatalog, IWriteStreamCatalog
{
    private readonly NuGetClient _nuGetClient = new("https://api.nuget.org/v3/index.json");
    public override string Title => "Plugins";

    protected override async Task<IEnumerable<Entry>> GetChildrenInternal()
    {
        if (CurrentPath! == string.Empty)
        {
            return [
            new Entry((PluginEntry.RootPath,PluginEntry.RootPath,-1, DateTime.MinValue, EntryCategory.Default),
                true, 
                this)];
        }
        
        string pluginsDirectory = MarketHelpers.PluginsDirectory;
        var installed = new Dictionary<string, NuGetVersion>(StringComparer.OrdinalIgnoreCase);
        if (Directory.Exists(pluginsDirectory))
        {
            foreach (string directory in Directory.EnumerateDirectories(pluginsDirectory))
            {
                string lockFile = Path.Combine(directory, "paket.lock");
                if (!File.Exists(lockFile)) 
                    throw new NotImplementedException("Handle a directory without metadata");
                
                string id = Path.GetFileName(directory);
                PackageResolver.ResolvedPackage? package = LockFile.LoadFrom(lockFile)
                    .GetGroup(Constants.MainDependencyGroup)
                    .Resolution
                    .Values
                    .FirstOrDefault(package => StringComparer.OrdinalIgnoreCase.Equals(package.Name.Name, id));
                if (package != null) installed[id] = NuGetVersion.Parse(package.Version.ToString());
                else throw new NotImplementedException($"Handle a paket file without id {id}");
            }
        }

        var versionResolver = new VersionFolderPathResolver(MarketHelpers.PackagesDirectory);
        IReadOnlyList<SearchResult> packages = await _nuGetClient.SearchAsync("tags:bashka-plugin");
        var updatesAvailable = new List<PluginEntry>();
        var latestInstalled = new List<PluginEntry>();
        var availableForInstallation = new List<PluginEntry>();
        foreach (SearchResult package in packages)
        {
            if (!installed.Remove(package.PackageId, out NuGetVersion? version))
            {
                availableForInstallation.Add(new PluginEntry(package, PluginStatus.Available, this));
                continue;
            }
            bool update = NuGetVersion.Parse(package.Version) > version;
            PluginEntry entry = CreateInstalledEntry(package, update ? PluginStatus.Update : PluginStatus.Installed, version);
            (update ? updatesAvailable : latestInstalled).Add(entry);
        }

        // installed, but not in NUGET feed
        IEnumerable<PluginEntry> localOnly = installed.Select(package =>
        {
            var metadata = new SearchResult
            {
                PackageId = package.Key, Version = package.Value.ToNormalizedString(),
                Versions = [new SearchResultVersion { Version = package.Value.ToNormalizedString() }]
            };
            string manifest = versionResolver.GetManifestFilePath(package.Key, package.Value);
            if (File.Exists(manifest))
            {
                using Stream stream = File.OpenRead(manifest);
                var nuspec = new NuspecReader(stream);
                metadata.PackageId = nuspec.GetId();
                metadata.Description = nuspec.GetDescription();
                metadata.Authors = nuspec.GetAuthors().Split(',',
                    StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
                metadata.ProjectUrl = nuspec.GetProjectUrl();
            }
            else throw new NotImplementedException("no manifest for an installed plugin");
            
            return CreateInstalledEntry(metadata, PluginStatus.Installed, package.Value, true);
        });
        return
        [
            .. localOnly, 
            .. updatesAvailable,
            .. latestInstalled,
            .. availableForInstallation
        ];

        PluginEntry CreateInstalledEntry(SearchResult package, PluginStatus status, NuGetVersion version, bool localOnly = false)
        {
            var archive = new FileInfo(versionResolver.GetPackageFilePath(package.PackageId, version));
            return new PluginEntry(package, status, this, version, localOnly, archive.Exists ? archive : null);
        }
    }

    public void WriteStream(string fullPath, Stream stream)
    {
        throw new NotImplementedException();
    }

    public Stream ReadStream(string fullPath)
    {
        throw new NotImplementedException();
    }
}
