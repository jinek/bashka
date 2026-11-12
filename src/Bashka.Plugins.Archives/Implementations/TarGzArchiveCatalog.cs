using System.IO.Compression;
using Bashka.Plugins.Core.Abstract.Data;

namespace Bashka.Plugins.Archives.Implementations;

/// <summary>
///     Gzip compressed tar, like .tar.gz or .tgz
/// </summary>
public class TarGzArchiveCatalog(Stream stream, Entry entry) : TarArchiveCatalog(stream, entry)
{
    protected override Stream PrepareStream(Stream stream)
    {
        var result = new MemoryStream();
        using (stream)
        {
            using var gZipStream = new GZipStream(stream, CompressionMode.Decompress);
            gZipStream.CopyTo(result);
        }

        result.Position = 0;
        return result;
    }
}