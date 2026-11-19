using System.Diagnostics.CodeAnalysis;
using Bashka.Plugins.Core.Abstract;
using Bashka.Plugins.Foundation.FileSystem;

namespace Bashka.Plugins.Foundation.ShellInteraction;

public sealed class SwitchConnectionEntry : FileEntry
{
    private readonly string _extension = string.Empty;

    [SetsRequiredMembers]
    public SwitchConnectionEntry(string name, ICatalog catalog, Func<IShellConnection> connectionFactory)
    {
        Path = string.Empty;
        FullPath = string.Empty;
        Name = name;
        Size = -1;
        TimeStamp = DateTime.Now;
        ParentCatalog = catalog;
        ConnectionFactory = connectionFactory;
    }

    [SetsRequiredMembers]
    public SwitchConnectionEntry(FileEntry fileEntry, ICatalog catalog, Func<IShellConnection> connectionFactory,
        string extension)
        : this(System.IO.Path.GetFileNameWithoutExtension(fileEntry.Name), catalog, connectionFactory)
    {
        Path = fileEntry.Path;
        FullPath = fileEntry.FullPath;
        Size = fileEntry.Size;
        TimeStamp = fileEntry.TimeStamp;
        Category = fileEntry.Category;
        Details = fileEntry.Details;
        _extension = extension;
    }

    public override string DisplayName => FormatDisplayName(Name);
    public override string Extension => _extension;
    public Func<IShellConnection> ConnectionFactory { get; }
}
