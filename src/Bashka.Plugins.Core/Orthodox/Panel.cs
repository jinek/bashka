using System.Diagnostics.CodeAnalysis;
using System.IO.Enumeration;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using Autofac.Features.Indexed;
using Bashka.Helpers;
using Bashka.Plugins.Core.Abstract;
using Bashka.Plugins.Core.Abstract.Data;
using Bashka.Plugins.Core.Abstract.Surrounding;
using Bashka.Plugins.Core.Scoping;
using DynamicData.Binding;
using JetBrains.Annotations;
using ReactiveUI.Avalonia;

namespace Bashka.Plugins.Core.Orthodox;

public class Panel : BaseObject, IPanel
{
    public IShellConnection ShellConnection { get; }
    public ICatalog RootsCatalog { get; }
    private readonly ICommandSelector _commandSelector;
    private readonly IIndex<string, ICatalog> _catalogsFactory;

    public IEnumerable<Entry> Roots => RootsCatalog.Children;

    public Panel(ICommandSelector commandSelector, IIndex<string, ICatalog> catalogsFactory, IShellConnection shellConnection)
    {
        ShellConnection = shellConnection;
        RootsCatalog = catalogsFactory[ICatalog.RootCatalogKey];
        _commandSelector = commandSelector;
        _catalogsFactory = catalogsFactory;

        this.WhenPropertyChanged(panel => panel.IsRootsVisible)
            .Skip(1)
            .ObserveOn(ThreadPoolScheduler.Instance) 
            .ObserveOn(AvaloniaScheduler.Instance)
            .Subscribe(b =>
            {
                if (b.Value) return;
                if (CurrentCatalog == null) return;
                if (SelectedIndex == -1)
                    SelectedIndex = 0;
                IsFocused = true;
            });
        
        this.WhenChanged(panel => panel.CurrentCatalog, panel => panel.CurrentCatalog!.Children,
                (panel, _, _) => panel.CurrentCatalog?.Children)
            .Subscribe(_ => { OnPropertyChanged(nameof(CurrentChildren)); });

        RootsCatalog.WhenPropertyChanged(catalog => catalog.Children)
            .Skip(1)
            .Subscribe(_ =>
            {
                OnPropertyChanged(nameof(Roots));
            });

        this.WhenPropertyChanged(panel => panel.CurrentCatalog)
            .Where(value => value.Value != null)
            .Subscribe(value =>
            {
                if (SelectedRoot == null)
                    return;
                
                if(SelectedRoot.ParentCatalog.TypeKey!=value.Value!.TypeKey)
                    SelectedRoot = null;
            });

        RootsCatalog.NavigateAsync(ICatalog.RootFavoritesPath).HandleErrors();
        
        OpenDefaultCatalogAsync().HandleErrors();
    }

    public async Task OpenDefaultCatalogAsync()
    {
        ICatalog catalog = _catalogsFactory[ICatalog.DefeaultCatalogTypeKey];
        _catalogStack.Clear();
        
        await catalog.NavigateAsync(await catalog.ResolveCurrentPathAsync());
        
        Push(catalog);
    }

    public void Push(ICatalog catalog)
    {
        _catalogStack.Push((catalog, SelectedIndex)); // SelectedIndex - index in previous catalog
        OnPropertyChanged(nameof(CurrentCatalog));
    }


    public bool IsRootsVisible { get; set; }

    private readonly Stack<(ICatalog, int)> _catalogStack = new();
    public ICatalog? CurrentCatalog => _catalogStack.TryPeek(out (ICatalog, int) peek) ? peek.Item1 : null;

    public void Pop()
    {
        (ICatalog _, int selectedIndex) = _catalogStack.Pop();
        OnPropertyChanged(nameof(CurrentCatalog));
        SelectedIndex = selectedIndex;
    }


    public IEnumerable<Entry>? CurrentChildren
    {
        get
        {
            if (CurrentCatalog == null)
                return null;

            GoBackEntry[]? goBackEntries = null;
            if (CurrentCatalog!.CanNavigateBack || _catalogStack.Count > 1)
            {
                goBackEntries = [new GoBackEntry(CurrentCatalog)];
            }

            IEnumerable<Entry> children = CurrentCatalog.Children;
            children = ApplySort(children);
            return goBackEntries?.Concat(children).ToArray() ?? children;
        }
    }

    public IReadOnlyList<Entry> MarkedEntries =>
        [.. CurrentCatalog?.Children.Where(entry => entry is { IsMarked: true, IsGone: false }) ?? []];

    public Entry? SelectedRoot { get; set; }
    public Entry? SelectedEntry { get; set; }
    public int SelectedIndex { get; set; }

    public SortMode SortMode { get; set; } = new(SortKey.Unsorted, false);

    [UsedImplicitly]
    public void OnSortModeChanged()
    {
        Entry? selectedEntry = SelectedEntry;
        OnPropertyChanged(nameof(CurrentChildren));
        if (selectedEntry == null) return;

        Entry[] children = [.. CurrentChildren ?? []];
        SelectedIndex = Array.IndexOf(children, selectedEntry);
    }

    private Entry[] ApplySort(IEnumerable<Entry> children)
    {
        if (SortMode.Key == SortKey.Unsorted)
            return [.. children];

        //directories always go first, like ls --group-directories-first
        IOrderedEnumerable<Entry> ordered = children.OrderByDescending(entry => entry.IsDirectory);

        ordered = (SortMode.Key, SortMode.Descending) switch
        {
            (SortKey.Name, false) => ordered.ThenBy(NameOf, StringComparer.OrdinalIgnoreCase),
            (SortKey.Name, true) => ordered.ThenByDescending(NameOf, StringComparer.OrdinalIgnoreCase),
            (SortKey.Extension, false) => ordered.ThenBy(ExtensionOf, StringComparer.OrdinalIgnoreCase)
                .ThenBy(NameOf, StringComparer.OrdinalIgnoreCase),
            (SortKey.Extension, true) => ordered.ThenByDescending(ExtensionOf, StringComparer.OrdinalIgnoreCase)
                .ThenByDescending(NameOf, StringComparer.OrdinalIgnoreCase),
            (SortKey.Size, false) => ordered.ThenBy(entry => entry.Size),
            (SortKey.Size, true) => ordered.ThenByDescending(entry => entry.Size),
            (SortKey.TimeStamp, false) => ordered.ThenBy(entry => entry.TimeStamp),
            (SortKey.TimeStamp, true) => ordered.ThenByDescending(entry => entry.TimeStamp),
            _ => ordered
        };

        return [.. ordered];

        static string NameOf(Entry entry) => entry.Name;
        static string ExtensionOf(Entry entry) => Path.GetExtension(entry.Name);
    }

    [UsedImplicitly]
    public async void OnSelectedRootChanged(Entry? _, Entry? newRoot)
    {
        if (newRoot == null)
            return;

        _catalogStack.Clear();
        var commandExecutionContext = new CommandExecutionContext([newRoot], RootsCatalog, this,
            Commander.Value, null);
        ICommand[] selectCommands =
            _commandSelector.SelectCommands(commandExecutionContext);
        ICommand command = selectCommands.Last(); //the latest registered in IOC overrides previous ones

        await command.ExecuteAsync(commandExecutionContext);
    }

    public async void EntrySelected()
    {
        var commandExecutionContext =
            new CommandExecutionContext([SelectedEntry!], CurrentCatalog, this, Commander.Value, null);
        ICommand[] selectCommands = _commandSelector.SelectCommands(commandExecutionContext);
        ICommand command = selectCommands.Last(); //the latest registered in IOC overrides previous ones
        await command.ExecuteAsync(commandExecutionContext);
    }

    public bool IsFocused { get; set; }

    public bool IsQuickSearchActive { get; set; }
    public string QuickSearchPattern { get; set; } = string.Empty;

    [UsedImplicitly]
    public void OnQuickSearchPatternChanged() => JumpToMatch(SelectedIndex, false);

    [UsedImplicitly]
    public void OnIsQuickSearchActiveChanged(bool _, bool newValue)
    {
        if (newValue) return;
        QuickSearchPattern = string.Empty;
        IsFocused = true;
    }

    public void QuickSearchNext() => JumpToMatch(SelectedIndex + 1, true);

    private void JumpToMatch(int startIndex, bool wrap)
    {
        if (!IsQuickSearchActive || QuickSearchPattern == string.Empty) return;

        Entry[] children = [.. CurrentChildren!];
        if (children.Length == 0) return;

        string pattern = QuickSearchPattern.EndsWith("*", StringComparison.Ordinal)
            ? QuickSearchPattern
            : QuickSearchPattern + '*';
        
        startIndex = Math.Max(startIndex, 0);
        int count = wrap ? children.Length : children.Length - startIndex;
        for (int offset = 0; offset < count; offset++)
        {
            int index = (startIndex + offset) % children.Length;
            if (FileSystemName.MatchesSimpleExpression(pattern, children[index].Name))
            {
                SelectedIndex = index;
                return;
            }
        }
    }
    
    [AutoWire]
    public Lazy<OrthodoxCommander> Commander { get; [UsedImplicitly] internal init; } = null!;
}