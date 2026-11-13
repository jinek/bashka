using BaGetter.Protocol;
using BaGetter.Protocol.Models;
using Bashka.Plugins.Core.Abstract.Data;
using Bashka.Plugins.Core.Abstract.Surrounding;
using Bashka.Plugins.Foundation.FileSystem;

namespace Bashka.Plugins.Market;

public sealed class PluginsCatalog() : PathCatalog, IReadStreamCatalog, IWriteStreamCatalog
{
    private readonly NuGetClient _nuGetClient = new("https://api.nuget.org/v3/index.json");
    public override string Title => "Plugins";

    protected override async Task<IEnumerable<Entry>> GetChildrenInternal()
    {
        if (CurrentPath! == string.Empty)
        {
            return [
            new Entry((PluginEntry.RootPath,PluginEntry.RootPath,-1, DateTime.MinValue, EntryCategory.Default),
                true, 
                this)];
        }
        
        IReadOnlyList<SearchResult> packages = await _nuGetClient.SearchAsync("tags:bashka-plugin");
        
        return
        [
            .. packages.Select(package => new PluginEntry(package, PluginStatus.Available, this))
        ];
    }

    public void WriteStream(string fullPath, Stream stream)
    {
        throw new NotImplementedException();
    }

    public Stream ReadStream(string fullPath)
    {
        throw new NotImplementedException();
    }
}
