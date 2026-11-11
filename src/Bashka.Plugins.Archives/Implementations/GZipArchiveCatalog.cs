using System.IO.Compression;
using Bashka.Plugins.Core.Abstract.Data;
using Bashka.Plugins.Core.Abstract.Surrounding;
using Bashka.Plugins.Foundation.FileSystem;

namespace Bashka.Plugins.Archives.Implementations;

public class GZipArchiveCatalog(Stream stream, Entry entry) : ArchiveCatalogBase(stream, entry)
{
    protected override IReadOnlyList<FileEntry> ReadAllEntries()
    {
        // Compressors like gzip pack a single file only, so such an archive is shown as a single entry catalog.
        string entryName = Path.GetFileNameWithoutExtension(ArchiveEntry.Name);
        return [CreateEntry(entryName, 0, ArchiveEntry.TimeStamp, false)];
    }

    public override Stream ReadStream(string fullPath)
    {
        if (fullPath != Entries[0].FullPath)
            throw new UserErrorException($"'{fullPath}' is not found in the archive");

        if (ArchiveStream.CanSeek)
            ArchiveStream.Position = 0;

        return new GZipStream(ArchiveStream, CompressionMode.Decompress, true);
    }
}
