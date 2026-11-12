namespace Bashka.Plugins.Core.Abstract.Data
{
    // ReSharper disable once UnusedTypeParameter //whats going on here
    public record CatalogCommandContext<TCatalog, TEntry>(CommandExecutionContext CommandExecutionContext)
        where TCatalog : ICatalog where TEntry : Entry
    {
        /// <summary>
        /// <see cref="CommandExecutionContext.Entries"/>
        /// </summary>
        public IReadOnlyList<TEntry> Entries => CommandExecutionContext.Entries.Cast<TEntry>().ToArray();

        /// <summary>
        /// The only entry of <see cref="Entries"/>, to be used by commands not supporting multiple entries
        /// </summary>
        public TEntry SingleEntry => (TEntry)CommandExecutionContext.Entries.Single();
    }
}