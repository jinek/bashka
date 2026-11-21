using Autofac;
using Avalonia;
using Avalonia.Controls;

namespace Bashka.Plugins.Core.Abstract;

public static class VisualExtensionExtensions
{
    /// <summary>
    ///     Resolves the theme and all plugin visual extensions from the ioc and registers their resources and styles in
    ///     the application.
    ///     Must be called before the main window content is built, resources are looked up already during that.
    ///     Registration is done application wide, because templates are also looked up dynamically
    ///     (see AppResourcesTemplateSelectorByType) outside of any window visual tree.
    /// </summary>
    public static void RegisterVisualExtensions(this Application application, ILifetimeScope scope)
    {
        application.Register(scope.ResolveKeyed<VisualExtension>(VisualExtension.ThemeKey));
        foreach (VisualExtension visualExtension in scope.Resolve<IEnumerable<VisualExtension>>())
            application.Register(visualExtension);
    }

    private static void Register(this Application application, VisualExtension visualExtension)
    {
        foreach (IResourceProvider resourceProvider in visualExtension.GetResources())
            application.Resources.MergedDictionaries.Add(resourceProvider);

        application.Styles.AddRange(visualExtension.GetStyles());
    }
}
