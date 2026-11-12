using Bashka.Plugins.Core.Abstract;
using Bashka.Plugins.Core.Abstract.Data;
using Bashka.Plugins.Core.Abstract.Surrounding;
using Bashka.Plugins.Core.Scoping;
using Bashka.Plugins.Foundation.FileSystem;
using Bashka.Plugins.Foundation.ShellInteraction;

namespace Bashka.Plugins.Archives.Bsdtar;

public class OpenBsdtarArchiveCommand()
    : CatalogCommand<ShellCatalog, FileEntry>(BsdtarSupport.SupportedExtensions, false, true)
{
    [AutoWire] internal Func<FileEntry, BsdtarArchiveCatalog> CatalogFactory { get; set; } = null!;

    public override async Task ExecuteAsync(CatalogCommandContext<ShellCatalog, FileEntry> context)
    {
        IPanel panel = context.CommandExecutionContext.Panel!;

        BsdtarArchiveCatalog archiveCatalog = CatalogFactory(context.SingleEntry);

        await archiveCatalog.NavigateAsync(string.Empty);
        panel.Push(archiveCatalog);
        panel.IsFocused = true;
        panel.SelectedIndex = 0;
    }
}