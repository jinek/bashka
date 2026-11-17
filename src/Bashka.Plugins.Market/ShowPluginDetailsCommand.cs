using Bashka.Plugins.Core.Abstract.Data;
using Bashka.Plugins.Core.Abstract.Surrounding;
using Bashka.Plugins.Core.Dialog.Abstract;
using Bashka.Plugins.Foundation;

namespace Bashka.Plugins.Market;

public sealed class ShowPluginDetailsCommand(IDialogProvider dialogProvider)
    : CatalogCommand<PluginsCatalog, PluginEntry>(null, false, true)
{
    public override Task ExecuteAsync(CatalogCommandContext<PluginsCatalog, PluginEntry> context)
    {
        return dialogProvider.ShowDialogAsync(new TextViewerDialogContent(
            context.SingleEntry.Name, context.SingleEntry.FormatMetadata()));
    }
}