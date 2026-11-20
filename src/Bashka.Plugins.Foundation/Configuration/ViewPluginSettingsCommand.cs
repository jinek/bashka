using Bashka.Plugins.Core.Abstract.Data;
using Bashka.Plugins.Core.Abstract.Surrounding;
using Bashka.Plugins.Core.Dialog.Abstract;
using Bashka.Plugins.Core.FunctionKeys;

namespace Bashka.Plugins.Foundation.Configuration;

public class ViewPluginSettingsCommand(IDialogProvider dialogProvider)
    : CatalogCommand<SettingsCatalog, PluginSettingsEntry>(null, false, true), IFunctionBarCatalogCommand
{
    public override Task ExecuteAsync(CatalogCommandContext<SettingsCatalog, PluginSettingsEntry> context)
    {
        PluginSettingsEntry entry = context.SingleEntry;
        return dialogProvider.ShowDialogAsync(new TextViewerDialogContent(entry.Name, entry.File.Content));
    }
}