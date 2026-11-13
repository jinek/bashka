using Bashka.Plugins.Core.Abstract;
using Bashka.Plugins.Core.Abstract.Data;
using Bashka.Plugins.Core.Abstract.Surrounding;
using Bashka.Plugins.Foundation.FileSystem;
using Bashka.Plugins.Core.Scoping;
using NuGet.Packaging.Core;
using BaGetter.Protocol.Models;
using NuGet.Versioning;

namespace Bashka.Plugins.Market;

public sealed class PluginsCatalog(IPluginFeed feed, PluginPackageStore store) : PathCatalog, IReadStreamCatalog, IWriteStreamCatalog
{
    private readonly Dictionary<string, NuGetVersion> _selectedVersions = new(StringComparer.OrdinalIgnoreCase);
    private IReadOnlyList<SearchResult> _remote = [];
    public const string RootPath = "plugins";

    [AutoWire] public PluginInstaller Installer { get; internal set; } = null!;

    public async Task InstallAsync(PluginEntry entry, NuGetVersion version)
    {
        using var timeout = new CancellationTokenSource(TimeSpan.FromMinutes(5));
        await Task.Run(() => Installer.InstallAsync(new PackageIdentity(entry.Package.Identity.Id, version), timeout.Token), timeout.Token);
        await SelectVersionAsync(entry, version);
    }

    public override string Title => FeedError == null ? "Plugins" : "Plugins (offline)";
    public string? FeedError { get; private set; }

    protected override async Task<IEnumerable<Entry>> GetChildrenInternal()
    {
        if (CurrentPath! == string.Empty)
            return [new Entry((RootPath, "Plugins", -1, DateTime.Now, EntryCategory.Default), true, this)];

        FeedError = null;
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(30));
        try
        {
            _remote = await feed.SearchAsync(timeout.Token);
        }
        catch (Exception exception) when (exception is HttpRequestException or PackageNotFoundException or OperationCanceledException)
        {
            FeedError = exception.Message;
        }
        return CreateEntries();
    }

    public Stream ReadStream(string fullPath)
    {
        PluginEntry entry = Children.OfType<PluginEntry>().FirstOrDefault(entry => entry.FullPath == fullPath)
            ?? throw new FileNotFoundException("Plugin package was not found.", fullPath);
        string temporary = Path.Combine(Path.GetTempPath(), $"bashka-download-{Guid.NewGuid():N}.nupkg");
        var stream = new FileStream(temporary, FileMode.CreateNew, FileAccess.ReadWrite, FileShare.None,
            81920, FileOptions.DeleteOnClose);
        try
        {
            using var timeout = new CancellationTokenSource(TimeSpan.FromMinutes(2));
            Task.Run(() => feed.DownloadAsync(entry.Package.Identity, stream, timeout.Token), timeout.Token)
                .GetAwaiter().GetResult();
            stream.Position = 0;
            return stream;
        }
        catch
        {
            stream.Dispose();
            throw;
        }
    }

    public void WriteStream(string fullPath, Stream stream)
    {
        if (Path.GetDirectoryName(fullPath) != RootPath
            || !fullPath.EndsWith(".nupkg", StringComparison.OrdinalIgnoreCase))
            throw new InvalidDataException("Plugin packages must be copied into the plugins root as .nupkg files.");
        using var timeout = new CancellationTokenSource(TimeSpan.FromMinutes(2));
        store.ImportAsync(stream, timeout.Token).GetAwaiter().GetResult();
    }

    public Task<IReadOnlyList<NuGetVersion>> GetVersionsAsync(PluginEntry entry)
    {
        string id = entry.Package.Identity.Id;
        IEnumerable<NuGetVersion> remote = _remote.Where(package =>
                StringComparer.OrdinalIgnoreCase.Equals(package.PackageId, id))
            .SelectMany(package => (package.Versions ?? []).Select(version => version.Version).Append(package.Version))
            .Select(NuGetVersion.Parse);
        IEnumerable<NuGetVersion> local = store.GetPackages().Where(package =>
                StringComparer.OrdinalIgnoreCase.Equals(package.Identity.Id, id))
            .Select(package => package.Identity.Version);
        return Task.FromResult<IReadOnlyList<NuGetVersion>>(remote.Concat(local)
            .Append(entry.Package.Identity.Version).Distinct().OrderDescending().ToArray());
    }

    public async Task SelectVersionAsync(PluginEntry entry, NuGetVersion version)
    {
        _selectedVersions[entry.Package.Identity.Id] = version;
        await NavigateAsync(string.Empty);
    }

    public void RestoreSelection(IPanel? panel, string packageId)
    {
        if (panel == null) return;
        panel.SelectedEntry = Children.OfType<PluginEntry>().FirstOrDefault(entry =>
            StringComparer.OrdinalIgnoreCase.Equals(entry.Package.Identity.Id, packageId));
        panel.IsFocused = true;
    }

    private IEnumerable<Entry> CreateEntries()
    {
        IReadOnlyList<PluginPackage> local = store.GetPackages();
        IEnumerable<PluginPackage> remote = _remote.SelectMany(result =>
            (result.Versions ?? []).Select(version => version.Version).Append(result.Version).Distinct()
                .Select(version => new PluginPackage(new PackageIdentity(result.PackageId, NuGetVersion.Parse(version)),
                    result.Description, string.Join(", ", result.Authors ?? []), DateTimeOffset.MinValue, -1,
                    result.ProjectUrl ?? string.Empty, [])));
        return local.Concat(remote).GroupBy(package => package.Identity.Id, StringComparer.OrdinalIgnoreCase)
            .OrderBy(group => group.Key, StringComparer.OrdinalIgnoreCase)
            .Select(group =>
            {
                NuGetVersion? preferred = _selectedVersions.GetValueOrDefault(group.Key)
                    ?? store.GetInstalledPlugins().FirstOrDefault(plugin =>
                        StringComparer.OrdinalIgnoreCase.Equals(plugin.Id, group.Key))?.Identity.Version
                    ?? local.Where(package => StringComparer.OrdinalIgnoreCase.Equals(package.Identity.Id, group.Key))
                        .Select(package => package.Identity.Version).OrderDescending().FirstOrDefault();
                PluginPackage package = group.FirstOrDefault(package => package.Identity.Version == preferred)
                    ?? group.MaxBy(package => package.Identity.Version)!;
                string status = store.IsInstalled(package.Identity) ? "Installed" : "Available";
                if (status == "Installed" && group.Any(other => other.Identity.Version > package.Identity.Version))
                    status = "Update";
                return new PluginEntry(package, status, this);
            }).ToArray();
    }
}
