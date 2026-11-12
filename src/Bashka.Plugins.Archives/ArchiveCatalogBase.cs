using Bashka.Plugins.Core.Abstract.Data;
using Bashka.Plugins.Core.Abstract.Surrounding;
using Bashka.Plugins.Foundation.FileSystem;

namespace Bashka.Plugins.Archives;

public abstract class ArchiveCatalogBase : PathCatalog, IArchiveCatalog, IDisposable
{
    private readonly Lazy<IReadOnlyList<FileEntry>> _allEntries;

    protected ArchiveCatalogBase(Stream stream, Entry entry)
    {
        ArchiveEntry = entry;
        ArchiveStream = PrepareStream(stream);
        _allEntries = new Lazy<IReadOnlyList<FileEntry>>(ReadAllEntries);
        NotifyDependentProperties(nameof(CurrentPath), nameof(Title));
    }

    protected Entry ArchiveEntry { get; }
    protected Stream ArchiveStream { get; }
    
    protected virtual Stream PrepareStream(Stream stream) => stream;

    protected abstract IReadOnlyList<FileEntry> ReadAllEntries();

    public abstract Stream ReadStream(string fullPath);

    protected IReadOnlyList<FileEntry> Entries => _allEntries.Value;
    
    protected FileEntry CreateEntry(string fullPath, long size, DateTime timeStamp, bool isDirectory)
    {
        string normalizedPath = ArchivePath.Normalize(fullPath);
        string name = Path.GetFileName(normalizedPath);

        return new FileEntry((name, name, normalizedPath, size, timeStamp,
            name.StartsWith('.') ? EntryCategory.Hidden : EntryCategory.Default), isDirectory, this);
    }

    public virtual void Dispose()
    {
        ArchiveStream.Dispose();
        GC.SuppressFinalize(this);
    }
    
    private string ShortArchiveName
    {
        get
        {
            const int maxArchiveNameLength = 20;
            string name = ArchiveEntry.Name;
            return name.Length <= maxArchiveNameLength ? name : $"...{name[^(maxArchiveNameLength - 3)..]}";
        }
    }

    public override string Title => string.IsNullOrEmpty(CurrentPath)
        ? ArchiveEntry.Name
        : $"{ShortArchiveName}{ArchivePath.Separator}{CurrentPath}";

    protected override Task<IEnumerable<Entry>> GetChildrenInternal()
    {
        string currentPath = CurrentPath!;
        string prefix = currentPath == string.Empty ? string.Empty : currentPath + ArchivePath.Separator;

        var directories = new Dictionary<string, FileEntry>();
        var files = new List<FileEntry>();

        foreach (FileEntry archiveEntry in Entries)
        {
            if (!archiveEntry.FullPath.StartsWith(prefix, StringComparison.Ordinal))
                continue;

            string relativePath = archiveEntry.FullPath[prefix.Length..];
            
            if (relativePath == string.Empty)
                //this directory itself
                continue;

            int separatorIndex = relativePath.IndexOf(ArchivePath.Separator);
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
                directories.Add(name, CreateEntry(prefix + name, 0, archiveEntry.TimeStamp, true));
        }

        return Task.FromResult<IEnumerable<Entry>>([
            .. directories.Values,
            .. files
        ]);
    }

    /// <summary>
    /// The root of the archive is an empty path, so the amount of path segments can not be used here
    /// </summary>
    public override bool CanNavigateBack => !string.IsNullOrEmpty(CurrentPath);
}
