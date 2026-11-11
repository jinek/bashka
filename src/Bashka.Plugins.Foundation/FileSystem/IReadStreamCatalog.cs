using Bashka.Plugins.Core.Abstract;

namespace Bashka.Plugins.Foundation.FileSystem;

public interface IReadStreamCatalog : ICatalog
{
    Stream ReadStream(string fullPath);
}