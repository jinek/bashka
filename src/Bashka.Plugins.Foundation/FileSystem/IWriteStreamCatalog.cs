using Bashka.Plugins.Core.Abstract;

namespace Bashka.Plugins.Foundation.FileSystem;

public interface IWriteStreamCatalog : ICatalog
{
    void WriteStream(string fullPath, Stream stream);
}