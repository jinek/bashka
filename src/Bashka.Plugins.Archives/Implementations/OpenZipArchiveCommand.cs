namespace Bashka.Plugins.Archives.Implementations;

/// <summary>
///     zip based formats
/// </summary>
public class OpenZipArchiveCommand() : OpenArchiveCommandBase<ZipArchiveCatalog>(@"(\.zip|\.jar|\.nupkg)$");