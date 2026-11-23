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

        var incompatiblePlugins = new List<(string Plugin, string Reason)>();
        var pluginErrors = new List<Exception>();
        PluginLoader.LoadInstalledPlugins(builder, (plugin, reason) => incompatiblePlugins.Add((plugin, reason)),
            pluginErrors.Add);

        if (DevelopmentHost.PluginModule is { } pluginModule)
            builder.RegisterModule(pluginModule);

        IContainer container = builder.Build();
        MainWindow.Scope = container.BeginLifetimeScope();
        MainWindow.IncompatiblePlugins = incompatiblePlugins.AsReadOnly();
        MainWindow.PluginLoadException = pluginErrors.Count == 0 ? null : new AggregateException(pluginErrors);
    }
}