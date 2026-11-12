using System.Globalization;
using System.Reactive;
using Autofac;
using Bashka.Plugins.Core.Abstract;
using Bashka.Plugins.Core.Abstract.Data;
using Bashka.Plugins.Core.Scoping;
using DynamicData.Binding;
using ReactiveUI;

namespace Bashka.Plugins.Core.Orthodox;

public class OrthodoxCommander : BaseObject, IUIRoot
{
    public FunctionKeys.FunctionBar FunctionBar { get; }
    private readonly Func<IShellConnection> _connectionFactory;
    private readonly KeyBindingService _keyBindingsService;
    public Interaction<Unit, Unit> FocusMenuInteraction { get; } = new();


    public OrthodoxCommander(Func<IShellConnection> connectionFactory, KeyBindingService keyBindingsService, FunctionKeys.FunctionBar functionBar)
    {
        FunctionBar = functionBar;
        _connectionFactory = connectionFactory;
        _keyBindingsService = keyBindingsService;
        /*ToolBoxItems = new[]
            {
                "  Help ", "UserMn", "View", "Edit", "Copy", "RenMov", "MkFold", "Delete", "ConfMn", "Quit", "Plugin",
                "Screen"
            }
            .Select((str, index) => new ToolboxItem { Name = str, Number = index + 1 }).ToArray();*/

        this.WhenChanged(navigator => navigator.LeftPanel!.IsFocused, 
                navigator => navigator.RightPanel!.IsFocused,
                (navigator, _, _) => navigator.LeftPanel!.IsFocused ^navigator.RightPanel!.IsFocused)
            .Subscribe(_ =>
            {
                OnPropertyChanged(nameof(CurrentPanel));
            });

        this.WhenPropertyChanged(navigator => navigator.CurrentPanel!.CurrentCatalog)
            .Subscribe(value =>
            {
                FunctionBar.CatalogType = value.Value?.GetType();
            });
        
        #region Updating FunctionBar

        // keeping dependency properties separate because it stops updating under various conditions
        
        this.WhenChanged(navigator => navigator.LeftPanel!.SelectedIndex,
                navigator => navigator.RightPanel!.SelectedIndex,
                navigator => navigator.LeftPanel!.IsFocused,
                navigator => navigator.RightPanel!.IsFocused,
                (_, _, _, _, _) => new object())
            .Subscribe(_ => RefreshFunctionBarContext());

        this.WhenChanged(navigator => navigator.LeftPanel!.SelectedEntry!.IsGone,
                (navigator, _) => navigator)
            .Subscribe(_ => RefreshFunctionBarContext());

        this.WhenChanged(navigator => navigator.RightPanel!.SelectedEntry!.IsGone,
                (navigator, _) => navigator)
            .Subscribe(_ => RefreshFunctionBarContext());

        void RefreshFunctionBarContext() => FunctionBar.Context = new CommandExecutionContext(this);
        #endregion
        
        KeyDownCommand = ReactiveCommand.Create<KeyDownModel>(KeyDown);
    }

    protected override void OnScopeInitialized()
    {
        StartPanels();
    }

    public void StartPanels()
    {
        //left
        RecreatePanel(null, _connectionFactory());
        LeftPanel!.IsFocused = true;
        //right
        RecreatePanel(null, _connectionFactory());
    }

    /// <summary>
    /// Can be null during initialization only
    /// </summary>
    public IPanel? LeftPanel { get; private set; }

    
    /// <summary>
    /// <see cref="LeftPanel"/>
    /// </summary>
    public IPanel? RightPanel { get; private set; }

    public IPanel RecreatePanel(IPanel? panelToRecreate, IShellConnection shellConnection)
    {
        ILifetimeScope newScope = LifetimeScope.BeginLifetimeScope(builder =>
        {
            builder.RegisterInstance(shellConnection).As<IShellConnection>();
        });
        var panelFactory = newScope.Resolve<Func<IPanel>>();
        
        bool isLeft = panelToRecreate == LeftPanel;
        IPanel newPanel = panelFactory();
        if (isLeft)
        {
            // null setups needed because of some avalonia binding issue
            LeftPanel = null!;
            LeftPanel = newPanel;
            OnPropertyChanged(nameof(LeftPanel));
        }
        else if (panelToRecreate == RightPanel)
        {
            RightPanel = null!;
            RightPanel = newPanel;
            OnPropertyChanged(nameof(RightPanel));
        }
        else throw new InvalidOperationException("Unknown panelToRecreate to change");

        return newPanel;
    }

    public IPanel? CurrentPanel => (RightPanel?.IsFocused??false) ? RightPanel : LeftPanel;
    public IPanel? AnotherPanel => (RightPanel?.IsFocused ?? false) ? LeftPanel : RightPanel;

    public ReactiveCommand<KeyDownModel, Unit> KeyDownCommand { get; }

    private async void KeyDown(KeyDownModel parameter)
    {
        ICommand[] commands = _keyBindingsService.MatchCommands(parameter.KeyCombination, nameof(OrthodoxCommander));
        foreach (ICommand command in commands)
        {
            var executionContext = new CommandExecutionContext(this);
            if (!command.CanExecute(executionContext)) continue;
            parameter.SetHandled();

            await command.ExecuteAsync(executionContext);
        }

        if (!parameter.Handled && _keyBindingsService.AltCharActivatesSearch &&
            parameter.KeyCombination is { Alt: true, Ctrl: false } &&
            TryGetChar(parameter.KeyCombination, out char ch) &&
            CurrentPanel is { IsQuickSearchActive: false, CurrentCatalog: not null } panel)
        {
            parameter.SetHandled();
            panel.IsQuickSearchActive = true;
            panel.QuickSearchPattern = ch.ToString();
        }

        return;

        static bool TryGetChar(KeyCombination combination, out char ch)
        {
            string key = combination.MainKey;
            ch = key switch
            {
                { Length: 1 } when char.IsLetter(key[0]) => combination.Shift ? key[0] : char.ToLower(key[0], CultureInfo.InvariantCulture),
                ['D', var digit] when char.IsDigit(digit) => digit,
                "MULTIPLY" => '*',
                "OEMPERIOD" => '.',
                "OEMMINUS" => '-',
                _ => '\0'
            };
            return ch != '\0';
        }
    }
}