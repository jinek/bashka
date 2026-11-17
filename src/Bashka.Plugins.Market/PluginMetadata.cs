using Bashka.Plugins.Core.Abstract.Data;
using NuGet.Packaging;
using NuGet.Versioning;
using Paket;

namespace Bashka.Plugins.Market;

internal sealed record PluginMetadata(NuGetVersion? InstalledVersion, long Size, DateTime TimeStamp)
{
    internal static PluginMetadata Read(string packageId)
    {
        string directory = Path.Combine(MarketHelpers.PluginsDirectory, packageId);
        if (!Directory.Exists(directory))
            return new PluginMetadata(null, -1, DateTime.MinValue);

        string lockFile = Path.Combine(directory, "paket.lock");
        if (!File.Exists(lockFile))
            throw new NotImplementedException("Handle a directory without metadata");

        PackageResolver.ResolvedPackage? package = LockFile.LoadFrom(lockFile)
            .GetGroup(Constants.MainDependencyGroup)
            .Resolution.Values
            .FirstOrDefault(package => StringComparer.OrdinalIgnoreCase.Equals(package.Name.Name, packageId));
        if (package == null)
            throw new NotImplementedException($"Handle a paket file without id {packageId}");

        NuGetVersion version = NuGetVersion.Parse(package.Version.ToString());
        var resolver = new VersionFolderPathResolver(MarketHelpers.PackagesDirectory);
        var archive = new FileInfo(resolver.GetPackageFilePath(packageId, version));
        return new PluginMetadata(version, archive.Exists ? archive.Length : -1,
            archive.Exists ? archive.LastWriteTime : DateTime.MinValue);
    }

    internal void ApplyTo(PluginEntry entry)
    {
        entry.InstalledVersion = InstalledVersion;
        entry.Status = InstalledVersion == null ? PluginStatus.Available
            : NuGetVersion.Parse(entry.Package.Version) > InstalledVersion ? PluginStatus.Update
            : PluginStatus.Installed;
        entry.Category = entry.LocalOnly ? EntryCategory.Important
            : entry.Status == PluginStatus.Installed ? EntryCategory.Default : EntryCategory.Executable;
        entry.Size = Size;
        entry.TimeStamp = TimeStamp;
        if (entry.LocalOnly && InstalledVersion == null && !entry.IsGone)
            entry.MarkDeleted();
    }

    internal static Task RefreshAsync(PluginEntry entry, TaskFactory uiTaskFactory)
    {
        PluginMetadata metadata = Read(entry.Package.PackageId);
        return uiTaskFactory.StartNew(() => metadata.ApplyTo(entry), CancellationToken.None);
    }
}