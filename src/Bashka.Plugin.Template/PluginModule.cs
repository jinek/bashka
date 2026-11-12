using Autofac;
using Bashka.Plugins.Core.Abstract;
using Bashka.Plugins.Core.FunctionBar;
using Bashka.Plugins.Core.Scoping;

namespace Bashka.Plugin.Template;

public sealed class PluginModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        builder.FullRegister<PluginVisualExtension, VisualExtension>();
        builder.RegisterFCommand<HelloWorldCommand, ICatalog>(FunctionPage.Default, FunctionKey.F1, "Hello");
    }
}
