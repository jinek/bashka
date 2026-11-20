using Bashka.Plugins.Core.Abstract;
using Bashka.Plugins.Core.Abstract.Data;
using Bashka.Plugins.Core.Configuration;
using Bashka.Plugins.Core.Scoping;
using Bashka.Plugins.Foundation.FileSystem;
using Bashka.Plugins.Foundation.ShellInteraction;

namespace Bashka.Plugins.Foundation.Configuration;

/// <summary>
///     Ini files <see cref="Settings" /> are merged from, in merge order: read-only files of plugins, user files, state.
///     User files and state are regular files
/// </summary>
public class SettingsCatalog(ICommandSelector commandSelector, Settings settings)
    : FileSystemCatalog(commandSelector)
{
    [AutoWire(Key = IShellConnection.DefaultShellConnectionKey)]
    public override ShellSession ShellSession { get; internal set; } = null!;

    public override string Title => "Settings";

    public override bool CanNavigateBack => false;

    public override Task<string> ResolveCurrentPathAsync()
    {
        return Task.FromResult(Settings.UserDirectory);
    }

    public override Task NavigateAsync(string path)
    {
        if (path != string.Empty) throw new NotSupportedException();
        Directory.CreateDirectory(Settings.UserDirectory);
        return base.NavigateAsync(Settings.UserDirectory);
    }

    public override Task NavigateBack()
    {
        throw new NotSupportedException();
    }

    protected override async Task<IEnumerable<Entry>> GetChildrenInternal()
    {
        IEnumerable<Entry> userFiles = await base.GetChildrenInternal();
        var state = new FileInfo(Settings.StateFilePath);
        return
        [
            .. settings.PluginFiles.Select(file => new PluginSettingsEntry(file, this)),
            .. userFiles.OfType<FileEntry>()
                .Where(entry => !entry.IsDirectory &&
                                entry.Name.EndsWith(".ini", StringComparison.OrdinalIgnoreCase))
                .OrderBy(entry => entry.Name, StringComparer.Ordinal)
                .Select(entry => new SettingsFileEntry(entry, "User", EntryCategory.Default)),
            .. state.Exists
                ? new[]
                {
                    new SettingsFileEntry(new FileEntry
                    {
                        Path = state.Name, Name = state.Name, FullPath = state.FullName, Size = state.Length,
                        TimeStamp = state.LastWriteTime, ParentCatalog = this
                    }, "State", EntryCategory.Important)
                }
                : []
        ];
    }
}