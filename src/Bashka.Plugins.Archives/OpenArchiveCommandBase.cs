using Bashka.Plugins.Core.Abstract;
using Bashka.Plugins.Core.Abstract.Data;
using Bashka.Plugins.Core.Abstract.Surrounding;
using Bashka.Plugins.Core.Scoping;
using Bashka.Plugins.Foundation.FileSystem;

namespace Bashka.Plugins.Archives;

public abstract class OpenArchiveCommandBase<TArchiveCatalogType>(string regexp)
    : CatalogCommand<IReadStreamCatalog, FileEntry>(regexp, false, true)
    where TArchiveCatalogType : IArchiveCatalog
{
    [AutoWire] internal Func<Stream, Entry, TArchiveCatalogType> CatalogFactory { get; set; } = null!;

    public override async Task ExecuteAsync(CatalogCommandContext<IReadStreamCatalog, FileEntry> context)
    {
        FileEntry archiveEntry = context.SingleEntry;
        IPanel panel = context.CommandExecutionContext.Panel!;

        IArchiveCatalog archiveCatalog =
            CatalogFactory(((IReadStreamCatalog)archiveEntry.ParentCatalog).ReadStream(archiveEntry.FullPath),
                archiveEntry);

        await archiveCatalog.NavigateAsync(string.Empty);
        panel.Push(archiveCatalog);
        panel.IsFocused = true;
        panel.SelectedIndex = 0;
    }
}