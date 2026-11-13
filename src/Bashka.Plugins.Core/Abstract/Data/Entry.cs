using Bashka.Plugins.Core.Scoping;
using PropertyChanged;

namespace Bashka.Plugins.Core.Abstract.Data;

public class Entry : BaseObject
{
    public Entry((string Path, string Name, long Size, DateTime TimeStamp, EntryCategory Category) pathAndName,
        bool isDirectory, ICatalog catalog)
    {
        ParentCatalog = catalog;
        PathAndName = pathAndName;
        IsDirectory = isDirectory;
        Size = pathAndName.Size;
        TimeStamp = pathAndName.TimeStamp;
        Category = pathAndName.Category;
    }

    public string Path => PathAndName.Path;
    public string Name => PathAndName.Name;
    public virtual string DisplayName => Name;
    public string Extension { get; init; } = string.Empty;
    public ICatalog ParentCatalog { get; }

    public long Size { get; init; }

    public DateTime TimeStamp { get; init; }
    public EntryCategory Category { get; }

    public (string Path, string Name, long Size, DateTime TimeStamp, EntryCategory Category) PathAndName
    {
        get;
        private set;
    }

    public bool IsDirectory { get; init; }

    /// <summary>
    ///     Additional information about the entry as received from the catalog read command (owner, group, permissions etc.)
    /// </summary>
    public IReadOnlyList<string> Details { get; init; } = [];

    /// <summary>
    ///     Entry does not exist in its parent catalog anymore, it was deleted or moved away
    /// </summary>
    [AlsoNotifyFor(nameof(SelfHack))]
    public bool IsGone { get; private set; }

    /// <summary>
    ///     Entry is selected as one of multiple entries (Far manager style selection)
    /// </summary>
    [AlsoNotifyFor(nameof(SelfHack))]
    public bool IsMarked { get; set; }

    public Entry SelfHack => this;

    public void MarkDeleted()
    {
        MarkGone("DELETED");
    }

    public void MarkMoved()
    {
        MarkGone("MOVED");
    }

    private void MarkGone(string marker)
    {
        if (IsGone)
            throw new NotImplementedException();

        IsGone = true;
        (string Path, string Name, long Size, DateTime TimeStamp, EntryCategory Category) pathAndName = PathAndName;

        pathAndName.TimeStamp = DateTime.Now;
        pathAndName.Name = $"[{marker}] {pathAndName.Name}";
        PathAndName = pathAndName;
    }
}
