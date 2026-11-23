using Bashka.Plugins.Core.Abstract;
using Bashka.Plugins.Core.Abstract.Data;
using Bashka.Plugins.Core.Scoping;

namespace Bashka.Plugins.Foundation.ShellInteraction;

public class LocalSystemConnectionCatalog(IShellConnection shellConnection) : BaseObject, ICatalog
{
    public string CurrentPath => string.Empty;
    public string Title => "▸Local system◂";
    public IEnumerable<Entry> Children { get; private set; } = [];

    public Task NavigateAsync(string path)
    {
        Children = shellConnection is LocalSystemShellConnection
            ? []
            :
            [
                new SwitchConnectionEntry("Reconnect to local system...", this,
                    () => new LocalSystemShellConnection() /*todo: resolve by typekey and same in ssh?*/)
            ];
        return Task.CompletedTask;
    }

    public bool CanNavigateBack => false;

    public Task NavigateBack()
    {
        throw new NotSupportedException();
    }
}