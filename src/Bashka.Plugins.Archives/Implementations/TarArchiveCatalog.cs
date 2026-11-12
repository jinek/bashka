using System.Formats.Tar;
using Bashka.Plugins.Core.Abstract.Data;
using Bashka.Plugins.Core.Abstract.Surrounding;
using Bashka.Plugins.Foundation.FileSystem;

namespace Bashka.Plugins.Archives.Implementations;

public class TarArchiveCatalog(Stream stream, Entry entry) : ArchiveCatalogBase(stream, entry)
{
    /// <summary>
    /// Tar has to be read from a seekable stream
    /// </summary>
    protected override Stream PrepareStream(Stream stream)
    {
        if (stream.CanSeek)
            return stream;

        var result = new MemoryStream();
        using (stream)
        {
            stream.CopyTo(result);
        }

        result.Position = 0;
        return result;
    }

    protected override IReadOnlyList<FileEntry> ReadAllEntries()
    {
        ArchiveStream!.Position = 0;
        using var tarReader = new TarReader(ArchiveStream, true);
        var result = new List<FileEntry>();
        while (tarReader.GetNextEntry() is { } tarEntry)
            result.Add(CreateEntry(
                tarEntry.Name,
                tarEntry.Length,
                tarEntry.ModificationTime.DateTime,
                tarEntry.EntryType is TarEntryType.Directory or TarEntryType.DirectoryList));

        return result;
    }

    public override Stream ReadStream(string fullPath)
    {
        ArchiveStream!.Position = 0;
        using var tarReader = new TarReader(ArchiveStream, true);
        while (tarReader.GetNextEntry() is { } tarEntry)
        {
            if (ArchivePath.Normalize(tarEntry.Name) != fullPath)
                continue;

            if (tarEntry.DataStream == null)
                break;

            //tar entry stream is valid only while the reader is alive, so the content is buffered
            var result = new MemoryStream();
            tarEntry.DataStream.CopyTo(result);
            result.Position = 0;
            return result;
        }

        throw new UserErrorException($"'{fullPath}' is not found in the archive");
    }
}
