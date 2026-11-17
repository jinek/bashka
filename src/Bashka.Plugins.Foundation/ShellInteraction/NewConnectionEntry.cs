using System.Diagnostics.CodeAnalysis;
using Bashka.Plugins.Core.Abstract;
using Bashka.Plugins.Core.Abstract.Data;

namespace Bashka.Plugins.Foundation.ShellInteraction;

public class NewConnectionEntry : Entry
{
    [SetsRequiredMembers]
    public NewConnectionEntry(ICatalog catalog)
    {
        Path = "";
        Name = "New Connection";
        Size = -1;
        TimeStamp = DateTime.Now;
        Category = EntryCategory.Executable;
        ParentCatalog = catalog;
    }
}