using Bashka.Plugins.Foundation.ShellInteraction;
using Bashka.Plugins.Foundation.ShellInteraction.Scripting;

namespace Bashka.Plugins.Archives.Bsdtar.Scripts;

/// <summary>
/// Creates an archive from the listed entries. Format is auto-detected by bsdtar from the archive extension (-a).
/// <paramref name="QuotedEntryNames"/> is a space separated list of already quoted names relative to <paramref name="WorkingDirectory"/>.
/// </summary>
[ShellScript(typeof(BashExecutor), """cd "{0}" && bsdtar -caf "{1}" -- {2}""")]
public record BsdtarCreateShellMultiScript(string WorkingDirectory, string ArchivePath, string QuotedEntryNames)
    : AttributedShellMultiScript(WorkingDirectory, ArchivePath, QuotedEntryNames);
