using Bashka.Plugins.Core.Abstract.Data;
using Bashka.Plugins.Core.Scoping;
using Bashka.Plugins.Core.Worker.Schedulers;

namespace Bashka.Plugins.Core.Abstract.Surrounding;

public class RootsCatalog : PathCatalog, IRootsCatalog
{
    private readonly Func<IEnumerable<ICatalog>> _catalogsFunction;
    private readonly UITaskFactory _iuiTaskFactory;
    private ICatalog[]? _catalogs;

    public RootsCatalog(Func<IEnumerable<ICatalog>> catalogsFunction, IUITaskFactory iuiTaskFactory)
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
                ICatalog[] catalogs = _catalogsFunction().Where(catalog => catalog.TypeKey != GetType().GetTypeKey())
                    .ToArray();
                return catalogs;
            });

            Task[] tasks = await _iuiTaskFactory.StartNew(() =>
            {
                //todo: can be merged into one task
                return _catalogs.Select(catalog => catalog.NavigateAsync(string.Empty)).ToArray();
            });

            await Task.WhenAll((IEnumerable<Task>)tasks);
        }

        const string catalogsPath = "catalogs";

        switch (CurrentPath)
        {
            case catalogsPath:
                return _catalogs.Select(catalog =>
                    new Entry
                    {
                        Path = string.Empty, Name = catalog.TypeKey, TimeStamp = DateTime.Now,
                        IsDirectory = true, ParentCatalog = catalog
                    });
            case ICatalog.RootFavoritesPath:
                Entry[] additionalEntries =
                {
                    new()
                    {
                        Path = catalogsPath, Name = "All catalogs", Size = -1, TimeStamp = DateTime.Now,
                        IsDirectory = true, ParentCatalog = this
                    }
                };
                Entry[] children = _catalogs.SelectMany(catalog => catalog.Children).ToArray();
                return children.Union(additionalEntries)
                    .GroupBy(entry => entry.ParentCatalog)
                    .SelectMany(group => group.Take(3))
                    .ToArray();

            default: throw new InvalidPathException(CurrentPath);
        }
    }
}