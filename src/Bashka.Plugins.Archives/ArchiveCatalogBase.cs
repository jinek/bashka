using Bashka.Plugins.Core.Abstract.Data;
using Bashka.Plugins.Core.Abstract.Surrounding;
using Bashka.Plugins.Foundation.FileSystem;

namespace Bashka.Plugins.Archives;

public abstract class ArchiveCatalogBase : PathCatalog, IArchiveCatalog, IDisposable
{
    private readonly Lazy<IReadOnlyList<FileEntry>> _allEntries;
    private readonly Stream _stream;

    protected ArchiveCatalogBase(Stream stream, Entry entry)
    {
        _stream = stream;
        ArchiveEntry = entry;
        _allEntries = new Lazy<IReadOnlyList<FileEntry>>(ReadAllEntries);
        NotifyDependentProperties(nameof(CurrentPath), nameof(Title));
    }

    protected Entry ArchiveEntry { get; }
    protected Stream? ArchiveStream { get; private set; }

    protected IReadOnlyList<FileEntry> Entries => _allEntries.Value;

    private string ShortArchiveName
    {
        get
        {
            const int maxArchiveNameLength = 20;
            string name = ArchiveEntry.Name;
            return name.Length <= maxArchiveNameLength ? name : $"...{name[^(maxArchiveNameLength - 3)..]}";
        }
    }

    public abstract Stream ReadStream(string fullPath);

    public override string Title => string.IsNullOrEmpty(CurrentPath)
        ? ArchiveEntry.Name
        : $"{ShortArchiveName}{ArchivePath.Separator}{CurrentPath}";

    /// <summary>
    ///     The root of the archive is an empty path, so the amount of path segments can not be used here
    /// </summary>
    public override bool CanNavigateBack => !string.IsNullOrEmpty(CurrentPath);

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual Stream PrepareStream(Stream stream)
    {
        return stream;
    }

    protected abstract IReadOnlyList<FileEntry> ReadAllEntries();

    protected FileEntry CreateEntry(string fullPath, long size, DateTime timeStamp, bool isDirectory)
    {
        string normalizedPath = ArchivePath.Normalize(fullPath);
        string name = Path.GetFileName(normalizedPath);

        return new FileEntry
        {
            Path = name, 
            Name = name, 
            FullPath = normalizedPath, 
            Size = size, 
            TimeStamp = timeStamp,
            Category = name.StartsWith('.') ? EntryCategory.Hidden : EntryCategory.Default,
            IsDirectory = isDirectory, 
            ParentCatalog = this
        };
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing) ArchiveStream?.Dispose();
    }

    protected override Task<IEnumerable<Entry>> GetChildrenInternal()
    {
        ArchiveStream ??= PrepareStream(_stream);

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
}