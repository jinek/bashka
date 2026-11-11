using System.IO.Compression;
using Bashka.Plugins.Core.Abstract.Data;
using Bashka.Plugins.Core.Abstract.Surrounding;
using Bashka.Plugins.Foundation.FileSystem;

namespace Bashka.Plugins.Archives.Implementations;

/// <summary>
/// Zip based formats, like .zip, .jar or .nupkg
/// </summary>
public class ZipArchiveCatalog : ArchiveCatalogBase
{
    private readonly ZipArchive _zipArchive;

    public ZipArchiveCatalog(Stream stream, Entry entry)
        : base(stream, entry)
    {
        _zipArchive = new ZipArchive(ArchiveStream, ZipArchiveMode.Read, false);
    }

    protected override IReadOnlyList<FileEntry> ReadAllEntries() =>
        _zipArchive.Entries
            .Select(zipEntry => CreateEntry(
                zipEntry.FullName,
                zipEntry.Length,
                zipEntry.LastWriteTime.DateTime,
                zipEntry.Name == string.Empty))
            .ToArray();

    public override Stream ReadStream(string fullPath)
    {
        ZipArchiveEntry zipArchiveEntry = _zipArchive.Entries
                                             .FirstOrDefault(archiveEntry =>
                                                 ArchivePath.Normalize(archiveEntry.FullName) == fullPath)
                                         ?? throw new UserErrorException($"'{fullPath}' is not found in the archive");
        return zipArchiveEntry.Open();
    }

    public override void Dispose()
    {
        _zipArchive.Dispose();
        base.Dispose();
    }
}
