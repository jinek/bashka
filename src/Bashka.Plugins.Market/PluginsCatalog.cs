using BaGetter.Protocol;
using BaGetter.Protocol.Models;
using Bashka.Plugins.Core.Abstract.Data;
using Bashka.Plugins.Core.Abstract.Surrounding;
using Bashka.Plugins.Foundation.FileSystem;
using NuGet.Packaging;

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
            new Entry
            {
                Path = PluginEntry.RootPath, 
                Name = PluginEntry.RootPath, 
                Size = -1,
                IsDirectory = true, 
                ParentCatalog = this
            }];
        }
        
        string pluginsDirectory = MarketHelpers.PluginsDirectory;
        var installed = new Dictionary<string, PluginMetadata>(StringComparer.OrdinalIgnoreCase);
        if (Directory.Exists(pluginsDirectory))
        {
            foreach (string directory in Directory.EnumerateDirectories(pluginsDirectory))
            {
                string id = Path.GetFileName(directory);
                installed.Add(id, PluginMetadata.Read(id));
            }
        }

        var versionResolver = new VersionFolderPathResolver(MarketHelpers.PackagesDirectory);
        IReadOnlyList<SearchResult> packages = await _nuGetClient.SearchAsync("tags:bashka-plugin");
        var updatesAvailable = new List<PluginEntry>();
        var latestInstalled = new List<PluginEntry>();
        var availableForInstallation = new List<PluginEntry>();
        foreach (SearchResult package in packages)
        {
            installed.Remove(package.PackageId, out PluginMetadata? metadata);
            PluginEntry entry = CreateEntry(package, metadata ?? PluginMetadata.Read(package.PackageId));
            switch (entry.Status)
            {
                case PluginStatus.Available: availableForInstallation.Add(entry); break;
                case PluginStatus.Update: updatesAvailable.Add(entry); break;
                case PluginStatus.Installed: latestInstalled.Add(entry); break;
            }
        }

        // installed, but not in NUGET feed
        IEnumerable<PluginEntry> localOnly = installed.Select(package =>
        {
            var metadata = new SearchResult
            {
                PackageId = package.Key, Version = package.Value.InstalledVersion!.ToNormalizedString(),
                Versions = [new SearchResultVersion { Version = package.Value.InstalledVersion!.ToNormalizedString() }]
            };
            string manifest = versionResolver.GetManifestFilePath(package.Key, package.Value.InstalledVersion!);
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
            
            return CreateEntry(metadata, package.Value, true);
        });
        return
        [
            .. localOnly, 
            .. updatesAvailable,
            .. latestInstalled,
            .. availableForInstallation
        ];

        PluginEntry CreateEntry(SearchResult package, PluginMetadata metadata, bool localOnly = false)
        {
            var entry = new PluginEntry
            {
                Path = package.PackageId, 
                Name = package.PackageId,
                FullPath = PathJoin(PluginEntry.RootPath, package.PackageId), 
                ParentCatalog = this,
                Package = package, 
                LocalOnly = localOnly
            };
            metadata.ApplyTo(entry);
            return entry;
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
