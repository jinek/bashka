using System.ComponentModel;
using Bashka.Plugins.Core.Abstract.Data;
using Bashka.Plugins.Core.Scoping;

namespace Bashka.Plugins.Core.Abstract;

public interface ICatalog : ITypeKeyedObject, INotifyPropertyChanged
{
    const string RootCatalogKey = "root_catalog";
    const string RootFavoritesPath = "favorites";
    const string DefeaultCatalogTypeKey = "default_catalog_type";
    const string SettingsCatalogKey = "settings_catalog";

    const string NameMetadataKey = "CatalogName";
    const string FavoritesCountMetadataKey = "CatalogFavoritesCount";

    string? CurrentPath { get; }
    string Title { get; }
    IEnumerable<Entry> Children { get; }
    bool CanNavigateBack { get; }

    /// <summary>
    ///     When true multiple entries can be marked (Far manager style selection) in this catalog
    /// </summary>
    bool AllowsMultipleSelection => true;

    /// <summary>
    ///     If
    ///     <param name="path" />
    ///     is empty, then just refreshes
    /// </summary>
    Task NavigateAsync(string path);

    Task<string> ResolveCurrentPathAsync()
    {
        return Task.FromResult(string.Empty);
    }

    Task NavigateBack();
}