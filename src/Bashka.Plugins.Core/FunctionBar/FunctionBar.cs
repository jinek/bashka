using Autofac;
using Autofac.Features.Metadata;
using Bashka.Plugins.Core.Abstract;
using Bashka.Plugins.Core.Abstract.Data;
using Bashka.Plugins.Core.Scoping;

namespace Bashka.Plugins.Core.FunctionBar;

public class FunctionBar : BaseObject
{
    /// <summary>
    /// injecting Ioc stuff to inspect types of registered commands
    /// </summary>
    public FunctionBar()
    {
        InitializeCatalogFunctionBar();
    }

    private Type? _catalogType;
    private CommandExecutionContext? _context;

    public FunctionPage CurrentPageIndex { get; set; }
    public FunctionBarPage[] AllPages { get; private set; }
    
    public FunctionBarPage CurrentPage => AllPages[(int)CurrentPageIndex];

    public Type? CatalogType
    {
        get => _catalogType;
        internal set
        {
            if (value == _catalogType) return;
            _catalogType = value;
            InitializeCatalogFunctionBar();
        }
    }

    public CommandExecutionContext? Context
    {
        get => _context;
        internal set
        {
            _context = value;
            foreach (FunctionBarPage fToolBoxPage in AllPages)
            {
                foreach (FunctionBarItem? fToolBoxItem in fToolBoxPage.Items)
                {
                    if (fToolBoxItem != null)
                    {
                        foreach (FunctionBarBinding commandWrapper in fToolBoxItem.CommandWrappers)
                        {
                            commandWrapper.Context = value;
                        }
                        
                        fToolBoxItem.ChooseCommand();
                    }
                }
            }
        }
    }

    private void InitializeCatalogFunctionBar()
    {
        if (_catalogType == null)
        {
            var emptySlot = new FunctionBarItem
            {
                CommandWrappers = Array.Empty<FunctionBarBinding>()
            };
            var emptyPage = new FunctionBarPage
            {
                Items = Enumerable.Repeat(emptySlot, 12).ToArray()
            };

            AllPages = Enumerable.Repeat(emptyPage, 4).ToArray();
            return;
        }

        //todo: instead of following code, just resolve IFunctionBarCommand<_catalogType> - need special registration on AutoFac side

        //IFunctionBarCommand<> is contravariant, so a command registered for a base catalog fits the current one too
        Type functionBarCommandType = typeof(IFunctionBarCommand<>).MakeGenericType(_catalogType);

        var all = LifetimeScope.Resolve<IEnumerable<Meta<ICommand>>>()
            .Where(command => command.Metadata.ContainsKey(nameof(FunctionPage))
                              && functionBarCommandType.IsInstanceOfType(command.Value))
            .Select(command => new
            {
                command = command.Value,
                fSlotPage = (FunctionPage)command.Metadata[nameof(FunctionPage)]!,
                fSlot = (FunctionKey)command.Metadata[nameof(FunctionKey)]!,
                fSlotName = (string)command.Metadata[IFunctionBarCommand<ICatalog>.DefaultTitleMetadataKey]!,
                isDelete = (bool)command.Metadata[IFunctionBarCommand<ICatalog>.IsDeleteMetadataKey]!
            }).ToArray();

        var allPages = new FunctionBarPage[4];

        for (int iPage = 0; iPage < 4; iPage++)
        {
            allPages[iPage] = new FunctionBarPage
            {
                Items = new FunctionBarItem[12]
            };

            for (int iFSlot = 0; iFSlot < 12; iFSlot++)
            {
                var allAssignedToSlot = Enumerable.Where(all, a => a!.fSlot == (FunctionKey)iFSlot && a.fSlotPage == (FunctionPage)iPage)
                    .ToArray();
                
                
                if (!Enumerable.Any(allAssignedToSlot)) continue;

                var item = new FunctionBarItem
                {
                    CommandWrappers = Enumerable.Select(allAssignedToSlot, arg => new FunctionBarBinding
                    {
                        IsDelete = arg.isDelete,
                        Command = arg.command,
                        Name = arg.fSlotName
                    }).ToArray<FunctionBarBinding>(),
                    Number = iFSlot + 1
                };
                allPages[iPage].Items[iFSlot] = item;
            }
        }
        
        AllPages = allPages;
    }
}