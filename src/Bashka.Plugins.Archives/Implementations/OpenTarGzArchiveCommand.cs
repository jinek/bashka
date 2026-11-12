namespace Bashka.Plugins.Archives.Implementations;

/// <summary>
///     gzip compressed tar
/// </summary>
public class OpenTarGzArchiveCommand() : OpenArchiveCommandBase<TarGzArchiveCatalog>(@"(\.tar\.gz|\.tgz)$");