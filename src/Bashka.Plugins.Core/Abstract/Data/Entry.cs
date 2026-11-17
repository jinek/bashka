using Bashka.Plugins.Core.Scoping;
using PropertyChanged;

namespace Bashka.Plugins.Core.Abstract.Data;

public class Entry : BaseObject
{
    private string? _goneMarker;

    public required string Path { get; init; }
    [AlsoNotifyFor(nameof(Extension))]
    public required string Name { get; init; }
    public virtual string DisplayName => FormatDisplayName(Name);
    public virtual string Extension { get; init; } = string.Empty;
    public required ICatalog ParentCatalog { get; init; }

    [AlsoNotifyFor(nameof(SelfHack))]
    public long Size { get; set; }

    public DateTime TimeStamp { get; set; }

    [AlsoNotifyFor(nameof(SelfHack))]
    public EntryCategory Category { get; set; }

    [AlsoNotifyFor(nameof(Extension), nameof(DisplayName))]
    public bool IsDirectory { get; init; }

    /// <summary>
    ///     Additional information about the entry as received from the catalog read command (owner, group, permissions etc.)
    /// </summary>
    public virtual IReadOnlyList<string> Details { get; init; } = [];

    /// <summary>
    ///     Entry does not exist in its parent catalog anymore, it was deleted or moved away
    /// </summary>
    [AlsoNotifyFor(nameof(SelfHack), nameof(DisplayName))]
    public bool IsGone { get; private set; }

    /// <summary>
    ///     Entry is selected as one of multiple entries (Far manager style selection)
    /// </summary>
    [AlsoNotifyFor(nameof(SelfHack))]
    public bool IsMarked { get; set; }

    public Entry SelfHack => this;

    protected string FormatDisplayName(string name) => _goneMarker == null ? name : $"[{_goneMarker}] {name}";

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

        _goneMarker = marker;
        IsGone = true;
        TimeStamp = DateTime.Now;
    }
}
