using BaGetter.Protocol.Models;
using Bashka.Plugins.Foundation.FileSystem;
using NuGet.Versioning;

namespace Bashka.Plugins.Market;

public sealed class PluginEntry : FileEntry
{
    internal const string RootPath = "plugins";

    public required SearchResult Package { get; init; }
    public PluginStatus Status { get; internal set; }
    public NuGetVersion? InstalledVersion { get; internal set; }
    internal bool LocalOnly { get; init; }

    public override string DisplayName => FormatDisplayName(Name);
    public override string Extension => Status == PluginStatus.Available ? string.Empty : Status.ToString();
    public override IReadOnlyList<string> Details => InstalledVersion == null
        ? [$"Version: {Package.Version}"]
        : [$"Version: {Package.Version}", $"Installed: {InstalledVersion.ToNormalizedString()}"];

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
