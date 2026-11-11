using Bashka.Plugins.Foundation.ShellInteraction;
using Bashka.Plugins.Foundation.ShellInteraction.Scripting;

namespace Bashka.Plugins.Archives.Bsdtar.Scripts;

/// <summary>
/// Extracts an archive (or only the listed entries when <paramref name="QuotedEntryPatterns"/> is not empty)
/// into the destination directory. <paramref name="QuotedEntryPatterns"/> is a space separated list of already quoted patterns.
/// </summary>
[ShellScript(typeof(BashExecutor), """mkdir -p "{1}" && bsdtar -xf "{0}" -C "{1}" {2}""")]
public record BsdtarExtractToDirectoryShellMultiScript(
    string ArchivePath,
    string DestinationDirectory,
    string QuotedEntryPatterns)
    : AttributedShellMultiScript(ArchivePath, DestinationDirectory, QuotedEntryPatterns);
