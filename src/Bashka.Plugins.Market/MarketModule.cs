using Autofac;
using Bashka.Plugins.Core.Abstract;
using Bashka.Plugins.Core.FunctionKeys;
using Bashka.Plugins.Core.Scoping;

namespace Bashka.Plugins.Market;

public sealed class MarketModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterType<NuGetPluginFeed>().As<IPluginFeed>().SingleInstance();
        builder.RegisterType<PluginPackageStore>().SingleInstance();
        builder.RegisterType<PluginInstaller>().SingleInstance();
        builder.FullRegister<PluginsCatalog, ICatalog>().AsSelf().InstancePerHierarchy();
        builder.FullRegister<MarketVisualExtension, VisualExtension>();
        builder.RegisterEnterCommand<InstallPluginCommand>();
        builder.RegisterFCommand<ShowPluginDetailsCommand, PluginsCatalog>(FunctionPage.Default, FunctionKey.F3, "View");
        builder.RegisterFCommand<CopyPluginPackageCommand, ICatalog>(FunctionPage.Default, FunctionKey.F5, "Copy");
        builder.RegisterFCommand<DeletePluginPackageCommand, PluginsCatalog>(FunctionPage.Default, FunctionKey.F8, "Uninstall", true);
    }
}
