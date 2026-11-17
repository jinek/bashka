using Autofac;
using Bashka.Plugins.Core.FunctionKeys;
using Bashka.Plugins.Core.Scoping;
using Bashka.Plugins.Foundation.FileSystem;
using Bashka.Plugins.Foundation.ShellInteraction;

namespace Bashka.Plugins.Archives.Bsdtar;

public sealed class PluginModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        builder.FullRegister<BsdtarArchiveCatalog, IReadStreamCatalog>();
        builder.RegisterEnterCommand<OpenBsdtarArchiveCommand>();
        builder.RegisterFCommand<AddToBsdtarArchiveCommand, ShellCatalog>(FunctionPage.Alt, FunctionKey.F5, "Pack");
        builder.RegisterFCommand<AddToBsdtarArchiveCommand, ShellCatalog>(FunctionPage.Shift, FunctionKey.F1, "Pack");
        builder.RegisterFCommand<ExtractBsdtarArchiveCommand, ShellCatalog>(FunctionPage.Shift, FunctionKey.F2,
            "UnPack");
        builder.RegisterFCommand<ExtractBsdtarEntriesCommand, BsdtarArchiveCatalog>(FunctionPage.Shift, FunctionKey.F2,
            "UnPack");
    }
}