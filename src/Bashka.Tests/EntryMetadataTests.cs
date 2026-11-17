using Bashka.Plugins.Core.Abstract.Data;
using Bashka.Plugins.Foundation.FileSystem;
using Bashka.Plugins.Market;

namespace Bashka.Tests;

[TestFixture]
public class EntryMetadataTests
{
    [TestCase("sample.txt", false, "sample", "txt")]
    [TestCase(".bashrc", false, ".bashrc", "")]
    [TestCase("folder.txt", true, "folder.txt", "")]
    [TestCase("archive.tar.gz", false, "archive.tar", "gz")]
    public void FileDisplayAndGoneMarkersNotifyBindings(string name, bool isDirectory, string displayName, string extension)
    {
        var entry = new FileEntry
        {
            Path = name, Name = name, FullPath = name, ParentCatalog = new PluginsCatalog(),
            IsDirectory = isDirectory
        };
        Assert.That(entry.DisplayName, Is.EqualTo(displayName));
        Assert.That(entry.Extension, Is.EqualTo(extension));
        var notifications = new HashSet<string?>();
        entry.PropertyChanged += (_, args) => notifications.Add(args.PropertyName);
        entry.Size = 123;
        entry.Category = EntryCategory.Important;
        entry.MarkMoved();
        Assert.That(entry.Name, Is.EqualTo(name));
        Assert.That(entry.Path, Is.EqualTo(name));
        Assert.That(entry.FullPath, Is.EqualTo(name));
        Assert.That(entry.DisplayName, Is.EqualTo("[MOVED] " + displayName));
        Assert.That(entry.IsGone, Is.True);
        Assert.That(entry.Extension, Is.EqualTo(extension));
        Assert.That(entry.TimeStamp, Is.GreaterThan(DateTime.MinValue));
        Assert.That(notifications, Does.Not.Contain("Name"));
        Assert.That(notifications, Does.Not.Contain("Extension"));
        Assert.Throws<NotImplementedException>(() => entry.MarkDeleted());
        Assert.That(notifications, Is.SupersetOf(new[] { "DisplayName", "Size", "Category", "SelfHack", "TimeStamp", "IsGone" }));
    }

    [TestCase(false)]
    [TestCase(true)]
    public void DeletedEntriesKeepIdentityAndNotifyDisplayName(bool plugin)
    {
        const string name = "Bashka.Example.Plugin";
        var catalog = new PluginsCatalog();
        Entry entry = plugin
            ? new PluginEntry
            {
                Path = name, Name = name, FullPath = "plugins/" + name, ParentCatalog = catalog,
                Package = new BaGetter.Protocol.Models.SearchResult { PackageId = name, Version = "1.0.0" }
            }
            : new Entry { Path = name, Name = name, ParentCatalog = catalog };
        string extension = entry.Extension;
        var notifications = new HashSet<string?>();
        entry.PropertyChanged += (_, args) => notifications.Add(args.PropertyName);

        entry.MarkDeleted();

        Assert.That(entry.Name, Is.EqualTo(name));
        Assert.That(entry.Path, Is.EqualTo(name));
        Assert.That(entry.DisplayName, Is.EqualTo("[DELETED] " + name));
        Assert.That(entry.Extension, Is.EqualTo(extension));
        Assert.That(entry.IsGone, Is.True);
        Assert.That(entry.TimeStamp, Is.GreaterThan(DateTime.MinValue));
        Assert.That(notifications, Does.Contain("DisplayName"));
        Assert.That(notifications, Does.Not.Contain("Name"));
        Assert.That(notifications, Does.Not.Contain("Extension"));
        Assert.Throws<NotImplementedException>(() => entry.MarkMoved());
    }

}
