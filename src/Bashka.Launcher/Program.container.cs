using Autofac;
using Autofac.Extras.AttributeMetadata;
using Bashka.Plugins.Archives;
using Bashka.Plugins.Core;
using Bashka.Plugins.Foundation;
using Bashka.Plugins.Market;

namespace Bashka.Launcher;

internal static partial class Program
{
    public static void ConfigureContainer()
    {
        var builder = new ContainerBuilder();

        builder.RegisterModule<AttributedMetadataModule>();
        builder.RegisterModule<CoreModule>();
        builder.RegisterModule<FoundationModule>();
        builder.RegisterModule<ArchivesModule>();
        builder.RegisterModule<MarketModule>();

        PluginLoader.LoadInstalledPlugins(builder);

        if (DevelopmentHost.PluginModule is { } pluginModule)
            builder.RegisterModule(pluginModule);

        IContainer container = builder.Build();
        MainWindow.Scope = container.BeginLifetimeScope();
    }
}