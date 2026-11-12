using Bashka.Plugins.Core.Abstract;
using Bashka.Plugins.Core.Abstract.Data;

namespace Bashka.Plugins.Foundation.FileSystem;

public class FileEntry : Entry
{
    public FileEntry(
        (string Path, string Name, string FullPath, long Size, DateTime TimeStamp, EntryCategory Catetgory) pathAndName,
        bool isDirectory, ICatalog catalog)
        : base((pathAndName.Path, pathAndName.Name, pathAndName.Size, pathAndName.TimeStamp, pathAndName.Catetgory),
            isDirectory, catalog)
    {
        FullPath = pathAndName.FullPath;
    }

    public string FullPath { get; }
}