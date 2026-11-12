using Bashka.Plugins.Core.Abstract.Data;
using Bashka.Plugins.Core.FunctionKeys;

namespace Bashka.Plugins.Core.Abstract.Surrounding;

public abstract class CatalogCommand<TCatalog, TEntry> : ICommand, IRegexCommand, IFunctionBarCommand<TCatalog>
    where TCatalog : ICatalog where TEntry : Entry
{
    public string? Regexp { get; }
    public bool Directory { get; }
    public bool File { get; }

    protected CatalogCommand(string? regexp, bool directory, bool file)
    {
        Regexp = regexp;
        Directory = directory;
        File = file;
    }

    /// <summary>
    /// When true the command can act on multiple <see cref="CommandExecutionContext.Entries"/>,
    /// otherwise it can only be executed when the context contains exactly one entry
    /// </summary>
    protected virtual bool SupportsMultipleEntries => false;

    public virtual bool CanExecute(CommandExecutionContext context)
    {
        IReadOnlyList<Entry> entries = context.Entries;
        if (entries.Count == 0)
            return false;
        if (!SupportsMultipleEntries && entries.Count != 1)
            return false;

        return entries.All(CanExecuteForEntry);
    }

    protected virtual bool CanExecuteForEntry(Entry entry)
    {
        if (entry.ParentCatalog is not TCatalog) return false;
        if (entry is not TEntry) return false;
        if (entry.IsGone) return false;

        if ((entry.IsDirectory && !Directory) || (!entry.IsDirectory && !File))
            return false;

        string entryPath = entry.Path;
        return Regexp == null || ((IRegexCommand)this).DoesRegexMatch(entryPath);
    }

    public Task ExecuteAsync(CommandExecutionContext context)
    {
        var catalogCommandContext = new CatalogCommandContext<TCatalog, TEntry>(context);
        return ExecuteAsync(catalogCommandContext);
    }

    public abstract Task ExecuteAsync(CatalogCommandContext<TCatalog, TEntry> context);
}