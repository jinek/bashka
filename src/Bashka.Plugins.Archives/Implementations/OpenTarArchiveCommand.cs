namespace Bashka.Plugins.Archives.Implementations;

/// <summary>
/// uncompressed tar
/// </summary>
public class OpenTarArchiveCommand() : OpenArchiveCommandBase<TarArchiveCatalog>(@"\.tar$");
