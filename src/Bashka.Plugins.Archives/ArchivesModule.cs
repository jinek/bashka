using Autofac;
using Bashka.Plugins.Archives.Implementations;
using Bashka.Plugins.Core.Scoping;

namespace Bashka.Plugins.Archives;

public sealed class ArchivesModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        builder.FullRegister<ZipArchiveCatalog, IArchiveCatalog>();
        builder.FullRegister<TarArchiveCatalog, IArchiveCatalog>();
        builder.FullRegister<TarGzArchiveCatalog, IArchiveCatalog>();
        builder.FullRegister<GZipArchiveCatalog, IArchiveCatalog>();
        builder.RegisterEnterCommand<OpenZipArchiveCommand>();
        builder.RegisterEnterCommand<OpenTarArchiveCommand>();
        builder.RegisterEnterCommand<OpenTarGzArchiveCommand>();
        builder.RegisterEnterCommand<OpenGZipArchiveCommand>();
    }
}
