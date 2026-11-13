using BaGetter.Protocol.Models;
using Bashka.Plugins.Core.Abstract.Data;
using Bashka.Plugins.Core.Abstract.Surrounding;
using Bashka.Plugins.Foundation.FileSystem;
using NuGet.Versioning;

namespace Bashka.Plugins.Market;

public sealed class PluginEntry : FileEntry
{
    internal const string RootPath = "plugins";

    public PluginEntry(SearchResult package, PluginStatus status, PluginsCatalog catalog,
        NuGetVersion? installedVersion = null, bool localOnly = false, FileInfo? installedPackage = null)
        : base((package.PackageId, package.PackageId, PathCatalog.PathJoin(RootPath, package.PackageId),
            installedPackage?.Length ?? -1, installedPackage?.LastWriteTime ?? DateTime.MinValue,
            localOnly ? EntryCategory.Important
                : status == PluginStatus.Installed ? EntryCategory.Default : EntryCategory.Executable), false, catalog)
    {
        Package = package;
        Status = status;
        InstalledVersion = installedVersion;
        Extension = status == PluginStatus.Available ? string.Empty : status.ToString();
    }

    public SearchResult Package { get; }
    public PluginStatus Status { get; }
    public NuGetVersion? InstalledVersion { get; }
    public override string DisplayName => Name;

    public string FormatMetadata() => $"""
        {Package.PackageId}
        Version: {Package.Version}
        {(InstalledVersion == null ? string.Empty : $"Installed version: {InstalledVersion.ToNormalizedString()}")}
        Status: {Status}
        Authors: {string.Join(", ", Package.Authors ?? [])}
        Project: {Package.ProjectUrl}

        {Package.Description}

        Versions:
        {string.Join(Environment.NewLine, (Package.Versions ?? []).Select(version => version.Version))}
        """;
}
