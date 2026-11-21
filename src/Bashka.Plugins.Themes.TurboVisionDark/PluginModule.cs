using Autofac;
using Bashka.Plugins.Core.Abstract;

namespace Bashka.Plugins.Themes.TurboVisionDark;

public sealed class PluginModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterType<TurboVisionDarkThemeExtension>().Keyed<VisualExtension>(VisualExtension.ThemeKey);
    }
}
