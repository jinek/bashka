using System.Globalization;
using Bashka.Plugins.Archives.Bsdtar.Scripts;
using Bashka.Plugins.Core.Abstract.Data;
using Bashka.Plugins.Foundation.FileSystem;
using Bashka.Plugins.Foundation.ShellInteraction;
using Bashka.Plugins.Foundation.ShellInteraction.Scripting.Scripts;

namespace Bashka.Plugins.Archives.Bsdtar;

/// <summary>
///     Formats are whatever "bsdtar" (libarchive) supports: 7z, rar, tar.bz2, tar.xz etc.
/// </summary>
public class BsdtarArchiveCatalog : ShellCatalog, IReadStreamCatalog
{
    private const char Separator = '/';
    private const int MaxArchiveNameLength = 20;


    public BsdtarArchiveCatalog(FileEntry archiveEntry)
    {
        ArchiveEntry = archiveEntry;
        NotifyDependentProperties(nameof(CurrentPath), nameof(Title));
    }

    internal FileEntry ArchiveEntry { get; }

    private string ShortArchiveName
    {
        get
        {
            string name = ArchiveEntry.Name;
            return name.Length <= MaxArchiveNameLength ? name : $"...{name[^(MaxArchiveNameLength - 3)..]}";
        }
    }

    public Stream ReadStream(string fullPath)
    {
        var content = new MemoryStream();
        using var writer = new StreamWriter(content, leaveOpen: true);

        foreach (CatLine catLine in ShellSession.PrepareQuery(
                     new BsdtarExtractShellMultiScript(ArchiveEntry.FullPath, fullPath)))
            writer.WriteLine(catLine.Line);

        writer.Flush();
        content.Position = 0;
        return content;
    }

    public override string Title => string.IsNullOrEmpty(CurrentPath)
        ? ArchiveEntry.Name
        : $"{ShortArchiveName}{Separator}{CurrentPath}";

    /// <summary>
    ///     The root of the archive is an empty path, so the amount of path segments can not be used here
    /// </summary>
    public override bool CanNavigateBack => !string.IsNullOrEmpty(CurrentPath);

    private FileEntry CreateEntry(RawBsdtarEntry rawEntry)
    {
        string entryPath = EntryPath(rawEntry.Name);
        string name = Path.GetFileName(entryPath);

        return new FileEntry((name, name, entryPath, long.Parse(rawEntry.Size),
                ParseTimeStamp(rawEntry.Month, rawEntry.Day, rawEntry.TimeOrYear),
                name.StartsWith('.') ? EntryCategory.Hidden : EntryCategory.Default),
            rawEntry.Type == "d", this);
    }

    private FileEntry CreateDirectoryEntry(string entryPath, DateTime timeStamp)
    {
        string name = Path.GetFileName(entryPath);
        return new FileEntry((name, name, entryPath, 0, timeStamp,
            name.StartsWith('.') ? EntryCategory.Hidden : EntryCategory.Default), true, this);
    }

    /// <summary>
    ///     Paths of the listing are converted to the paths of entries: slash separated, without a leading
    ///     "./" of tar based formats and without a trailing slash of directories. Symlinks are listed as "link -&gt; target".
    /// </summary>
    private static string EntryPath(string listedName)
    {
        string result = listedName;

        int symlinkArrowIndex = result.IndexOf(" -> ", StringComparison.Ordinal);
        if (symlinkArrowIndex >= 0)
            result = result[..symlinkArrowIndex];

        result = result.Trim(Separator);

        while (result.StartsWith($".{Separator}", StringComparison.Ordinal))
            result = result[2..].Trim(Separator);

        return result == "." ? string.Empty : result;
    }

    private static DateTime ParseTimeStamp(string month, string day, string timeOrYear)
    {
        string text = timeOrYear.Contains(':')
            ? $"{month} {day} {DateTime.Now.Year} {timeOrYear}"
            : $"{month} {day} {timeOrYear}";

        return DateTime.TryParse(text, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime result)
            ? result
            : DateTime.Now;
    }

    protected override Task<IEnumerable<Entry>> GetChildrenInternal()
    {
        //todo: low: this duplictaes ArchiveCatalogBase
        string currentPath = CurrentPath!;
        string prefix = currentPath == string.Empty ? string.Empty : currentPath + Separator;

        var directories = new Dictionary<string, FileEntry>();
        var files = new List<FileEntry>();

        IReadOnlyList<FileEntry> allEntries =
        [
            .. ShellSession.PrepareQuery(new BsdtarListShellMultiScript(ArchiveEntry.FullPath)).Select(CreateEntry)
        ];

        foreach (FileEntry archiveEntry in allEntries)
        {
            if (!archiveEntry.FullPath.StartsWith(prefix, StringComparison.Ordinal))
                continue;

            string relativePath = archiveEntry.FullPath[prefix.Length..];

            if (relativePath == string.Empty)
                //this directory itself
                continue;

            int separatorIndex = relativePath.IndexOf(Separator);
            if (separatorIndex < 0)
            {
                if (archiveEntry.IsDirectory)
                    directories.TryAdd(relativePath, archiveEntry);
                else
                    files.Add(archiveEntry);
                continue;
            }

            //a directory can be listed by an own entry and by entries inside of it, so it's deduplicated
            string name = relativePath[..separatorIndex];
            if (!directories.ContainsKey(name))
                directories.Add(name, CreateDirectoryEntry(prefix + name, archiveEntry.TimeStamp));
        }

        return Task.FromResult<IEnumerable<Entry>>([
            .. directories.Values,
            .. files
        ]);
    }
}