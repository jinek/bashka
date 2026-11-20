using System.Diagnostics.CodeAnalysis;
using Bashka.Plugins.Core.Abstract.Data;
using Bashka.Plugins.Foundation.FileSystem;

namespace Bashka.Plugins.Foundation.Configuration;

/// <summary>
///     User or state ini file
/// </summary>
public sealed class SettingsFileEntry : FileEntry
{
    private readonly string _kind;

    [SetsRequiredMembers]
    public SettingsFileEntry(FileEntry file, string kind, EntryCategory category)
    {
        _kind = kind;
        Path = file.Path;
        Name = file.Name;
        FullPath = file.FullPath;
        Size = file.Size;
        TimeStamp = file.TimeStamp;
        Details = file.Details;
        Category = category;
        ParentCatalog = file.ParentCatalog;
    }

    public override string DisplayName =>
        FormatDisplayName($"{System.IO.Path.GetFileNameWithoutExtension(Name)} ({_kind})");
}