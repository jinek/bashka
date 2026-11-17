using System.Diagnostics.CodeAnalysis;
using Bashka.Plugins.Core.Abstract;
using Bashka.Plugins.Core.Abstract.Data;

namespace Bashka.Plugins.Core.Orthodox;

public class SwitchConnectionEntry : Entry
{
    [SetsRequiredMembers]
    public SwitchConnectionEntry(string name, ICatalog catalog, Func<IShellConnection> connectionFactory)
    {
        Path = string.Empty;
        Name = name;
        Size = -1;
        TimeStamp = DateTime.Now;
        ParentCatalog = catalog;
        ConnectionFactory = connectionFactory;
    }

    public Func<IShellConnection> ConnectionFactory { get; }
}