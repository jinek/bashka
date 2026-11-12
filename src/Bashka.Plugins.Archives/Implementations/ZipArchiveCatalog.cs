using System.IO.Compression;
using Bashka.Plugins.Core.Abstract.Data;
using Bashka.Plugins.Core.Abstract.Surrounding;
using Bashka.Plugins.Foundation.FileSystem;

namespace Bashka.Plugins.Archives.Implementations;

/// <summary>
///     Zip based formats, like .zip, .jar or .nupkg
/// </summary>
public class ZipArchiveCatalog : ArchiveCatalogBase
{
    private ZipArchive? _zipArchive;

    public ZipArchiveCatalog(Stream stream, Entry entry)
        : base(stream, entry)
    {
    }

    protected override IReadOnlyList<FileEntry> ReadAllEntries()
    {
        _zipArchive ??= new ZipArchive(ArchiveStream!, ZipArchiveMode.Read, false);
        return _zipArchive.Entries
            .Select(zipEntry => CreateEntry(
                zipEntry.FullName,
                zipEntry.Length,
                zipEntry.LastWriteTime.DateTime,
                zipEntry.Name == string.Empty))
            .ToArray();
    }

    public override Stream ReadStream(string fullPath)
    {
        ZipArchiveEntry zipArchiveEntry = _zipArchive!.Entries
                                              .FirstOrDefault(archiveEntry =>
                                                  ArchivePath.Normalize(archiveEntry.FullName) == fullPath)
                                          ?? throw new UserErrorException($"'{fullPath}' is not found in the archive");
        return zipArchiveEntry.Open();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
            _zipArchive?.Dispose();
        base.Dispose(disposing);
    }
}