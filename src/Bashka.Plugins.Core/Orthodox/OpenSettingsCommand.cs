using Autofac.Features.Indexed;
using Bashka.Plugins.Core.Abstract;
using Bashka.Plugins.Core.Abstract.Data;

namespace Bashka.Plugins.Core.Orthodox;

/// <summary>
///     Enters the settings catalog in the current panel like a root
/// </summary>
public class OpenSettingsCommand(IIndex<string, ICatalog> catalogsFactory) : ICommand
{
    public bool CanExecute(CommandExecutionContext context)
    {
        return context.Panel != null;
    }

    public Task ExecuteAsync(CommandExecutionContext context)
    {
        IPanel panel = context.Panel!;
        return panel.EnterAsync(new Entry
        {
            Path = string.Empty,
            Name = "Settings",
            IsDirectory = true,
            ParentCatalog = catalogsFactory[ICatalog.SettingsCatalogKey]
        }, panel.CurrentCatalog);
    }
}