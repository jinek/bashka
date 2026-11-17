using System.Diagnostics.CodeAnalysis;
using Bashka.Plugins.Core.Abstract.Data;

namespace Bashka.Plugins.Core.Abstract.Surrounding;

public class GoBackEntry : Entry
{
    [SetsRequiredMembers]
    public GoBackEntry(ICatalog catalog)
    {
        Path = "..";
        Name = "..";
        Size = 0;
        TimeStamp = DateTime.Now;
        Category = EntryCategory.Default;
        ParentCatalog = catalog;
    }
}
