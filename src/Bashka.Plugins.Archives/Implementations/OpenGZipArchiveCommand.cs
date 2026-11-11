namespace Bashka.Plugins.Archives.Implementations;

public class OpenGZipArchiveCommand() : OpenArchiveCommandBase<GZipArchiveCatalog>(@"(?<!\.tar)\.gz$");
