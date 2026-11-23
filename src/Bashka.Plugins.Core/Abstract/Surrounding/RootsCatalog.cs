using Autofac.Features.Metadata;
using Bashka.Plugins.Core.Abstract.Data;
using Bashka.Plugins.Core.Scoping;
using Bashka.Plugins.Core.Worker.Schedulers;

namespace Bashka.Plugins.Core.Abstract.Surrounding;

public class RootsCatalog : PathCatalog, IRootsCatalog
{
    private const string EverythingPath = "everything";
    private readonly Func<IEnumerable<Meta<ICatalog>>> _catalogsFunction;
    private readonly UITaskFactory _iuiTaskFactory;
    private (ICatalog Catalog, string Name, int FavoritesCount)[]? _catalogs;

    public RootsCatalog(Func<IEnumerable<Meta<ICatalog>>> catalogsFunction, IUITaskFactory iuiTaskFactory)
    {
        _catalogsFunction = catalogsFunction;
        _iuiTaskFactory = (UITaskFactory)iuiTaskFactory;
    }

    public override string Title => "Roots";

    protected override async Task<IEnumerable<Entry>> GetChildrenInternal()
    {
        if (_catalogs == null)
        {
            _catalogs = await _iuiTaskFactory.StartNew(() =>
            {
                return _catalogsFunction()
                    .Where(meta => meta.Value.TypeKey != GetType().GetTypeKey())
                    .Select(meta => (meta.Value,
                        (string)RequiredMetadata(meta, ICatalog.NameMetadataKey),
                        (int)RequiredMetadata(meta, ICatalog.FavoritesCountMetadataKey)))
                    .ToArray();

                static object RequiredMetadata(Meta<ICatalog> meta, string key)
                {
                    return meta.Metadata[key] ?? throw new InvalidOperationException($"Metadata not set for {key}");
                }
            });

            Task[] tasks = await _iuiTaskFactory.StartNew(() =>
            {
                //todo: can be merged into one task
                return _catalogs.Where(catalog => catalog.FavoritesCount > 0)
                    .Select(catalog => catalog.Catalog.NavigateAsync(string.Empty))
                    .ToArray();
            });

            await Task.WhenAll((IEnumerable<Task>)tasks);
        }

        return CurrentPath switch
        {
            EverythingPath => _catalogs.Select(catalog => CatalogEntry(catalog.Catalog, catalog.Name)).ToArray(),
            ICatalog.RootFavoritesPath =>
            [
                .. _catalogs.SelectMany(catalog => catalog.FavoritesCount switch
                {
                    > 0 => catalog.Catalog.Children.Take(catalog.FavoritesCount),
                    0 => [CatalogEntry(catalog.Catalog, catalog.Name)],
                    _ => []
                }),
                new Entry
                {
                    Path = EverythingPath, Name = EverythingPath, Size = -1, TimeStamp = DateTime.Now,
                    IsDirectory = true, ParentCatalog = this
                }
            ],
            _ => throw new InvalidPathException(CurrentPath)
        };

        static Entry CatalogEntry(ICatalog catalog, string name)
        {
            return new Entry
            {
                Path = string.Empty, Name = name, Size = -1, TimeStamp = DateTime.Now, IsDirectory = true,
                ParentCatalog = catalog
            };
        }
    }
}
