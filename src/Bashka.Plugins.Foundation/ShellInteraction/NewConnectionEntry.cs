using Bashka.Plugins.Core.Abstract;
using Bashka.Plugins.Core.Abstract.Data;

namespace Bashka.Plugins.Foundation.ShellInteraction;

public class NewConnectionEntry(ICatalog catalog)
    : Entry((string.Empty, "New Connection", -1, DateTime.Now, EntryCategory.Executable), false, catalog);