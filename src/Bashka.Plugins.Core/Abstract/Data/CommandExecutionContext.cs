using System.Runtime.CompilerServices;
using Bashka.Plugins.Core.Orthodox;

namespace Bashka.Plugins.Core.Abstract.Data
{
    /// <param name="Entries">Entries the command should act on: the marked entries when any, otherwise the focused entry</param>
    public record CommandExecutionContext(IReadOnlyList<Entry> Entries,
        ICatalog? Catalog,
        IPanel? Panel,
        OrthodoxCommander? OrthodoxCommander,
        IPanel? AnotherPanel,
        ICatalog? AnotherCatalog = null)
    {
        public CommandExecutionContext(OrthodoxCommander orthodoxCommander) : this([], null, null, orthodoxCommander, null)
        {
            IPanel panel = orthodoxCommander.CurrentPanel!;
            Panel = panel;
            Catalog = panel.CurrentCatalog;
            AnotherPanel = orthodoxCommander.AnotherPanel;
            AnotherCatalog = AnotherPanel?.CurrentCatalog;

            IReadOnlyList<Entry> markedEntries = panel.MarkedEntries;
            Entries = markedEntries.Count > 0
                ? markedEntries
                : panel.SelectedEntry == null ? [] : [panel.SelectedEntry];
        }
        
        public virtual bool Equals(CommandExecutionContext? other) => ReferenceEquals(this, other);
        public override int GetHashCode() => RuntimeHelpers.GetHashCode(this);
    }
}
