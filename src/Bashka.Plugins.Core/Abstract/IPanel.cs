using System.ComponentModel;
using Bashka.Plugins.Core.Abstract.Data;

namespace Bashka.Plugins.Core.Abstract;

public interface IPanel : INotifyPropertyChanged
{
    void Push(ICatalog catalog);

    void Pop();

    IEnumerable<Entry>? CurrentChildren { get; }

    ICatalog? CurrentCatalog { get; }
    IEnumerable<Entry> Roots { get; }
    bool IsRootsVisible { get; set; }
    Entry? SelectedRoot { get; set; }
    Entry? SelectedEntry { get; set; }

    /// <summary>
    /// Entries marked in the current catalog (Far manager style selection)
    /// </summary>
    IReadOnlyList<Entry> MarkedEntries { get; }
    int SelectedIndex { get; set; }
    bool IsFocused { get; set; }
    SortMode SortMode { get; set; }
    ICatalog RootsCatalog { get; }
    IShellConnection ShellConnection { get; }
    void EntrySelected();
    
    bool IsQuickSearchActive { get; set; }
    string QuickSearchPattern { get; set; }
    void QuickSearchNext();
}