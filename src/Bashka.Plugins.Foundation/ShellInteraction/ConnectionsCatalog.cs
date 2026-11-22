using Bashka.Helpers;
using Bashka.Plugins.Core.Abstract;
using Bashka.Plugins.Core.Abstract.Data;
using Bashka.Plugins.Core.Scoping;
using Bashka.Plugins.Foundation.FileSystem;

namespace Bashka.Plugins.Foundation.ShellInteraction;

/// <summary>
///     SSH Connections
/// </summary>
public class ConnectionsCatalog : FileSystemCatalog
{
    public static readonly string ConnectionsDirectory = Path.Combine(BashkaHelpers.DataDirectory, "Connections");

    public ConnectionsCatalog(ICommandSelector commandSelector) : base(commandSelector)
    {
        Directory.CreateDirectory(ConnectionsDirectory);
    }

    [AutoWire(Key = IShellConnection.DefaultShellConnectionKey)]
    public override ShellSession ShellSession { get; internal set; } = null!;

    public override string Title => "Connections";

    public override bool CanNavigateBack => false;

    public override Task<string> ResolveCurrentPathAsync()
    {
        return Task.FromResult(ConnectionsDirectory);
    }

    public override Task NavigateBack()
    {
        throw new NotSupportedException();
    }

    protected override async Task<IEnumerable<Entry>> GetChildrenInternal()
    {
        if (CurrentPath == string.Empty)
            return
            [
                new Entry
                {
                    Path = ConnectionsDirectory, 
                    Name = "ssh", 
                    Size = -1, 
                    TimeStamp = DateTime.Now,
                    IsDirectory = true, 
                    ParentCatalog = this
                }
            ];

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