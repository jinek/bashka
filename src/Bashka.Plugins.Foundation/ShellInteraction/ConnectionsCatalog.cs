using Autofac;
using Bashka.Helpers;
using Bashka.Plugins.Core.Abstract;
using Bashka.Plugins.Core.Abstract.Data;
using Bashka.Plugins.Core.Scoping;
using Bashka.Plugins.Foundation.FileSystem;

namespace Bashka.Plugins.Foundation.ShellInteraction;

/// <summary>
///     SSH Connections
/// </summary>
public class ConnectionsCatalog : BaseObject, IPtyExecutor
{
    private const string Connections = "Connections";

    public static readonly string ConnectionsDirectory = Path.Combine(BashkaHelpers.DataDirectory, Connections);

    private readonly FileSystemCatalog _inner;

    public ConnectionsCatalog(ILifetimeScope scope)
    {
        ILifetimeScope localSystemScope = scope.BeginLifetimeScope(builder =>
        {
            var shellConnection = scope.ResolveKeyed<IShellConnection>(IShellConnection.DefaultShellConnectionKey);

            builder.RegisterInstance(shellConnection);
        });

        _inner = localSystemScope.Resolve<FileSystemCatalog>();
        Directory.CreateDirectory(ConnectionsDirectory);
        _inner.PropertyChanged += (_, args) => OnPropertyChanged(args.PropertyName);
    }

    public string CurrentPath => Connections;

    public string Title => Connections;

    public Task<string> ResolveCurrentPathAsync()
    {
        return Task.FromResult(ConnectionsDirectory);
    }

    public IEnumerable<Entry> Children
    {
        get
        {
            return
            [
                new NewConnectionEntry(this), .._inner.Children
                    .Select(ToConnectionEntry)
                    .Where(entry => entry != null)
                    .Cast<Entry>()
            ];
        }
    }

    public Task NavigateAsync(string path)
    {
        return path != string.Empty
            ? throw new NotSupportedException()
            : _inner.NavigateAsync(ConnectionsDirectory);
    }

    public bool CanNavigateBack => false;

    public Task NavigateBack()
    {
        throw new NotSupportedException();
    }

    public Task ExecuteAsResidentAsync(string workingDir, string applicationName, string arguments,
        CancellationToken cancellationToken)
    {
        return _inner.ExecuteAsResidentAsync(workingDir, applicationName, arguments, cancellationToken);
    }

    private Entry? ToConnectionEntry(Entry entry)
    {
        if (entry is not FileEntry { IsDirectory: false } fileEntry ||
            !fileEntry.Name.EndsWith(".ini", StringComparison.OrdinalIgnoreCase))
            return null;

        SshConnectionSettings settings = SshConnectionSettings.Load(fileEntry.FullPath);
        return new SwitchConnectionEntry(fileEntry, this,
            () => new SshShellConnection(SshConnectionSettings.Load(fileEntry.FullPath).ToConnectionInfo()),
            string.IsNullOrWhiteSpace(settings.Host) ? string.Empty : $"{settings.Host}:{settings.Port}");
    }
}
