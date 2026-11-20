using System.Diagnostics.CodeAnalysis;
using Bashka.Plugins.Core.Abstract;
using Bashka.Plugins.Core.Abstract.Data;
using Bashka.Plugins.Core.Configuration;

namespace Bashka.Plugins.Foundation.Configuration;

/// <summary>
///     Not a file entry, so file commands (copy, move, delete...) are not available for it
/// </summary>
public sealed class PluginSettingsEntry : Entry
{
    [SetsRequiredMembers]
    public PluginSettingsEntry(PluginSettingsFile file, ICatalog catalog)
    {
        File = file;
        Path = file.ResourceName;
        Name = file.ResourceName;
        Extension = System.IO.Path.GetExtension(file.ResourceName).TrimStart('.');
        Size = file.Content.Length;
        TimeStamp = System.IO.File.Exists(file.Assembly.Location)
            ? System.IO.File.GetLastWriteTime(file.Assembly.Location)
            : DateTime.Now;
        Category = EntryCategory.Hidden;
        ParentCatalog = catalog;
    }

    public PluginSettingsFile File { get; }

    public override string DisplayName =>
        FormatDisplayName($"{System.IO.Path.GetFileNameWithoutExtension(Name)} ({File.PluginName})");
}