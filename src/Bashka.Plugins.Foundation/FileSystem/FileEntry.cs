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
        string extension = System.IO.Path.GetExtension(Name);
        if (!isDirectory && extension.Length > 0 && extension.Length < Name.Length)
            Extension = extension[1..];
    }

    public string FullPath { get; }
    public override string DisplayName => Extension.Length == 0 ? Name : System.IO.Path.GetFileNameWithoutExtension(Name);
}
