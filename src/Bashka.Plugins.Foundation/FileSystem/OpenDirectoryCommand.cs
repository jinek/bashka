using Autofac.Features.Indexed;
using Bashka.Plugins.Core.Abstract;
using Bashka.Plugins.Core.Abstract.Data;
using Bashka.Plugins.Core.Abstract.Surrounding;

namespace Bashka.Plugins.Foundation.FileSystem;

public class OpenDirectoryCommand : CatalogCommand<ICatalog, Entry>
{
    private readonly IIndex<string, ICatalog> _catalogsFactory;

    public override bool CanExecute(CommandExecutionContext context)
    {
        return context.Entries.All(entry => entry.IsDirectory) && base.CanExecute(context);
    }

    public override async Task ExecuteAsync(CatalogCommandContext<ICatalog, Entry> context)
    {
        bool needsCatalogSwitch = false;
        IPanel panel = context.CommandExecutionContext.Panel!;
        Entry entry = context.SingleEntry;
        ICatalog catalog = entry.ParentCatalog;
        panel.SelectedIndex = -1;

        if (panel.CurrentCatalog != catalog)
        {
            catalog = _catalogsFactory[catalog.TypeKey];
            needsCatalogSwitch = true;
        }
        
        await catalog.NavigateAsync(entry.Path);

        if (needsCatalogSwitch)
        {
            //in the case of root have to change
            panel.Push(catalog);
        }

        panel.IsFocused = true;
        panel.SelectedIndex = 0;
        //panel.RaisePropertyChanged(new PropertyChangedEventArgs("CurrentChildren"));
        /*panel.CurrentCatalog.RaisePropertyChanged(new PropertyChangedEventArgs(nameof(ICatalog.Children)));*/
    }

    public OpenDirectoryCommand(IIndex<string, ICatalog> catalogsFactory) : base(null, true, false)
    {
        _catalogsFactory = catalogsFactory;
    }
}