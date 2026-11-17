using Bashka.Plugins.Core.Abstract.Data;

namespace Bashka.Plugins.Foundation.FileSystem;

public class FileEntry : Entry
{
    public required string FullPath { get; init; }

    public override string Extension
    {
        get
        {
            string extension = System.IO.Path.GetExtension(Name);
            return !IsDirectory && extension.Length > 0 && extension.Length < Name.Length
                ? extension[1..]
                : string.Empty;
        }
    }

    public override string DisplayName => FormatDisplayName(
        Extension.Length == 0 ? Name : System.IO.Path.GetFileNameWithoutExtension(Name));
}