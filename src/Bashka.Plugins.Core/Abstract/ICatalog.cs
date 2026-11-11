using System.ComponentModel;
using Bashka.Plugins.Core.Abstract.Data;
using Bashka.Plugins.Core.Scoping;

namespace Bashka.Plugins.Core.Abstract;

public interface ICatalog : ITypeKeyedObject, INotifyPropertyChanged 
{
    string CurrentPath { get; }
    string Title { get; }
    IEnumerable<Entry> Children { get; }
    
    /// <summary>
    /// If <param name="path"/> is empty, then just refreshes
    /// </summary>
    Task NavigateAsync(string path);
    bool CanNavigateBack { get; }

    Task<string> ResolveCurrentPathAsync() => Task.FromResult(string.Empty);

    /// <summary>
    /// When true multiple entries can be marked (Far manager style selection) in this catalog
    /// </summary>
    bool AllowsMultipleSelection => true;
    Task NavigateBack();
    const string RootCatalogKey = "root_catalog";
    const string RootFavoritesPath = "favorites";
    const string DefeaultCatalogTypeKey = "default_catalog_type";
}