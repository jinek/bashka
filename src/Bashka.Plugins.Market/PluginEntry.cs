using BaGetter.Protocol.Models;
using Bashka.Plugins.Core.Abstract.Data;
using Bashka.Plugins.Core.Abstract.Surrounding;
using Bashka.Plugins.Foundation.FileSystem;

namespace Bashka.Plugins.Market;

public sealed class PluginEntry : FileEntry
{
    internal const string RootPath = "plugins";

    public PluginEntry(SearchResult package, PluginStatus status, PluginsCatalog catalog)
        : base((package.PackageId, package.PackageId, PathCatalog.PathJoin(RootPath, package.PackageId),
            -1, DateTime.MinValue,
            status == PluginStatus.Installed ? EntryCategory.Default : EntryCategory.Executable), false, catalog)
    {
        Package = package;
        Status = status;
        Extension = status == PluginStatus.Available ? string.Empty : status.ToString();
    }

    public SearchResult Package { get; }
    public PluginStatus Status { get; }
    public override string DisplayName => Name;

    public string FormatMetadata() => $"""
        {Package.PackageId}
        Version: {Package.Version}
        Status: {Status}
        Authors: {string.Join(", ", Package.Authors ?? [])}
        Project: {Package.ProjectUrl}

        {Package.Description}

        Versions:
        {string.Join(Environment.NewLine, (Package.Versions ?? []).Select(version => version.Version))}
        """;
}
