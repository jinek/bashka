using System.IO.Enumeration;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using Autofac.Core;
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
    private readonly Stack<(ICatalog, int)> _catalogStack = new();
    private readonly IIndex<string, ICatalog> _catalogsFactory;
    private readonly ICommandSelector _commandSelector;
    private readonly bool _isLeft = OrthodoxCommander.CreatingLeftPanel.CurrentValue;
    private readonly OrthodoxCommanderSettings _settings;
    private readonly OrthodoxCommanderState _state;
    private string? _defaultCatalogTypeKey;

    public Panel(ICommandSelector commandSelector,
        IIndex<string, ICatalog> catalogsFactory,
        IShellConnection shellConnection,
        OrthodoxCommanderSettings settings,
        OrthodoxCommanderState state)
    {
        _settings = settings;
        _state = state;
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

        // vibe-voded subscription, caution!
        this.WhenPropertyChanged(panel => panel.CurrentChildren)
            .Select(children => (children.Value ?? [])
                .Select(entry => entry.WhenAnyPropertyChanged(nameof(Entry.IsMarked), nameof(Entry.IsGone)))
                .Merge()
                .Select(_ => Unit.Default)
                .StartWith(Unit.Default))
            .Switch()
            .Subscribe(_ => { OnPropertyChanged(nameof(MarkedEntries)); });

        RootsCatalog.WhenPropertyChanged(catalog => catalog.Children)
            .Skip(1)
            .Subscribe(_ => { OnPropertyChanged(nameof(Roots)); });

        this.WhenPropertyChanged(panel => panel.CurrentCatalog)
            .Where(value => value.Value != null)
            .Subscribe(value =>
            {
                if (SelectedRoot == null)
                    return;

                if (SelectedRoot.ParentCatalog.TypeKey != value.Value!.TypeKey)
                    SelectedRoot = null;
            });

        this.WhenChanged(panel => panel.CurrentCatalog!.CurrentPath, (panel, _) => panel.CurrentCatalog)
            .Where(catalog => catalog is { CurrentPath.Length: > 0 } && catalog.TypeKey == _defaultCatalogTypeKey &&
                              IsOnDefaultConnection)
            .Subscribe(catalog => SavedLocation = catalog!.CurrentPath);
    }

    [AutoWire] public Lazy<OrthodoxCommander> Commander { get; [UsedImplicitly] internal init; } = null!;

    private string? StartupLocation => _isLeft ? _settings.StartupLocationLeft : _settings.StartupLocationRight;

    private string? SavedLocation
    {
        get => _isLeft ? _state.LeftPath : _state.RightPath;
        set
        {
            if (_isLeft) _state.LeftPath = value;
            else _state.RightPath = value;
        }
    }

    private bool IsOnDefaultConnection =>
        LifetimeScope.ComponentRegistry.TryGetServiceRegistration(
            new KeyedService(IShellConnection.DefaultShellConnectionKey, typeof(IShellConnection)),
            out ServiceRegistration registration) &&
        registration.Registration.Activator.LimitType == ShellConnection.GetType();

    public IShellConnection ShellConnection { get; }
    public ICatalog RootsCatalog { get; }

    public IEnumerable<Entry> Roots => RootsCatalog.Children;

    public void Push(ICatalog catalog)
    {
        _catalogStack.Push((catalog, SelectedIndex)); // SelectedIndex - index in previous catalog
        OnPropertyChanged(nameof(CurrentCatalog));
    }


    public bool IsRootsVisible { get; set; }
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
                goBackEntries = [new GoBackEntry(CurrentCatalog)];

            IEnumerable<Entry> children = CurrentCatalog.Children;
            children = ApplySort(children);
            return goBackEntries?.Concat(children).ToArray() ?? children;
        }
    }

    public IReadOnlyList<Entry> MarkedEntries =>
        (CurrentCatalog?.Children ?? []).Where(entry => entry is { IsMarked: true, IsGone: false })
        .ToArray()
        .AsReadOnly();

    public Entry? SelectedRoot { get; set; }
    public Entry? SelectedEntry { get; set; }
    public int SelectedIndex { get; set; }

    public SortMode SortMode { get; set; } = new(SortKey.Unsorted, false);

    public async void EntrySelected()
    {
        await EnterAsync(SelectedEntry!, CurrentCatalog);
    }

    public async Task EnterAsync(Entry entry, ICatalog? catalog)
    {
        var commandExecutionContext = new CommandExecutionContext([entry], catalog, this, Commander.Value, null);
        ICommand command = _commandSelector.SelectCommands(commandExecutionContext)
            .Last(); //the latest registered in IOC overrides previous ones
        await command.ExecuteAsync(commandExecutionContext);
    }

    public bool IsFocused { get; set; }

    public bool IsQuickSearchActive { get; set; }
    public string QuickSearchPattern { get; set; } = string.Empty;

    public void QuickSearchNext()
    {
        JumpToMatch(SelectedIndex + 1, true);
    }

    private async void InitializeCatalogs()
    {
        await Task.WhenAll(RootsCatalog.NavigateAsync(ICatalog.RootFavoritesPath), OpenDefaultCatalogAsync());
    }

    public async Task OpenDefaultCatalogAsync()
    {
        ICatalog catalog = _catalogsFactory[ICatalog.DefeaultCatalogTypeKey];
        _defaultCatalogTypeKey = catalog.TypeKey;
        _catalogStack.Clear();

        await catalog.NavigateAsync(IsOnDefaultConnection
                                    && BashkaHelpers.IsStartedByLauncher
                                    && (StartupLocation ?? SavedLocation) is { Length: > 0 } location
            ? location
            : await catalog.ResolveCurrentPathAsync());

        Push(catalog);
    }

    protected override void OnScopeInitialized()
    {
        InitializeCatalogs();
    }

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

        static string NameOf(Entry entry)
        {
            return entry.Name;
        }

        static string ExtensionOf(Entry entry)
        {
            return entry.Extension;
        }
    }

    [UsedImplicitly]
    public async void OnSelectedRootChanged(Entry? _, Entry? newRoot)
    {
        if (newRoot == null)
            return;

        _catalogStack.Clear();
        try
        {
            await EnterAsync(newRoot, RootsCatalog);
        }
        finally
        {
            // focusing datagrid again
            IsFocused = false;
            IsFocused = true;
        }
    }

    [UsedImplicitly]
    public void OnQuickSearchPatternChanged()
    {
        JumpToMatch(SelectedIndex, false);
    }

    [UsedImplicitly]
    public void OnIsQuickSearchActiveChanged(bool _, bool newValue)
    {
        if (newValue) return;
        QuickSearchPattern = string.Empty;
        IsFocused = true;
    }

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
}