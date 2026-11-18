using Autofac;
using Bashka.Helpers;
using Bashka.Plugins.Core.Abstract;
using Bashka.Plugins.Core.Abstract.Data;
using Bashka.Plugins.Foundation.FileSystem;

namespace Bashka.Plugins.Foundation.ShellInteraction;

/// <summary>
///     SSH Connections
/// </summary>
public class ConnectionsCatalog : FileSystemCatalog
{
    public static readonly string ConnectionsDirectory = Path.Combine(BashkaHelpers.DataDirectory, "Connections");

    private readonly ShellSession _localShellSession;

    public ConnectionsCatalog(ILifetimeScope scope, ICommandSelector commandSelector) : base(commandSelector)
    {
        ILifetimeScope localSystemScope = scope.BeginLifetimeScope(builder =>
        {
            var shellConnection = scope.ResolveKeyed<IShellConnection>(IShellConnection.DefaultShellConnectionKey);
            builder.RegisterInstance(shellConnection);
        });

        _localShellSession = localSystemScope.Resolve<ShellSession>();
        Directory.CreateDirectory(ConnectionsDirectory);
    }

    public override string Title => "Connections";

    public override Task<string> ResolveCurrentPathAsync()
    {
        return Task.FromResult(ConnectionsDirectory);
    }

    public override Task NavigateAsync(string path)
    {
        if (path != string.Empty) throw new NotSupportedException();
        ShellSession = _localShellSession;
        return base.NavigateAsync(ConnectionsDirectory);
    }

    public override bool CanNavigateBack => false;

    public override Task NavigateBack()
    {
        throw new NotSupportedException();
    }

    protected override async Task<IEnumerable<Entry>> GetChildrenInternal()
    {
        IEnumerable<Entry> files = await base.GetChildrenInternal();
        return
        [
            new NewConnectionEntry(this), .. files.Select(ToConnectionEntry).OfType<SwitchConnectionEntry>()
        ];
    }

    private SwitchConnectionEntry? ToConnectionEntry(Entry entry)
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
