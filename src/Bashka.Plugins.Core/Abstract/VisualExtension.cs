using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Styling;

namespace Bashka.Plugins.Core.Abstract;

/// <summary>
///     Visual entry point of a plugin. Everything visual a plugin wants to bring to the application
///     (styles, themes, resource dictionaries, converters, data templates etc) is exposed here
///     and is registered in the window at its startup. By default the visuals are brought as embedded
///     avares axaml files.
/// </summary>
public abstract class VisualExtension
{
    /// <summary>
    ///     Key of the base theme. Only one is applied: the latest registered one overrides the previously registered.
    ///     It is applied before other visual extensions
    /// </summary>
    public const string ThemeKey = "base_theme_visual_extension";

    /// <summary>
    ///     Uris of axaml files containing styles, like "avares://MyPlugin/UI/MyStyles.axaml".
    /// </summary>
    protected virtual IEnumerable<string> StyleUris => [];

    /// <summary>
    ///     Uris of axaml files containing resource dictionaries, like "avares://MyPlugin/UI/MyResources.axaml".
    /// </summary>
    protected virtual IEnumerable<string> ResourceUris => [];

    public virtual IEnumerable<IStyle> GetStyles()
    {
        return StyleUris.Select(Load<IStyle>);
    }

    public virtual IEnumerable<IResourceProvider> GetResources()
    {
        return ResourceUris.Select(Load<IResourceProvider>);
    }

    protected static T Load<T>(string uri)
    {
        return (T)AvaloniaXamlLoader.Load(new Uri(uri));
    }

    protected static T Load<T>(Uri uri)
    {
        return (T)AvaloniaXamlLoader.Load(uri);
    }
}