using Bashka.Plugins.Foundation.ShellInteraction;
using Bashka.Plugins.Foundation.ShellInteraction.Scripting;
using Bashka.Plugins.Foundation.ShellInteraction.Scripting.Scripts;

namespace Bashka.Plugins.Archives.Bsdtar.Scripts;

[ShellScript(typeof(BashExecutor), """bsdtar -xOf "{0}" "{1}"; echo""", "^(?<Line>.+)$")]
public record BsdtarExtractShellMultiScript(string ArchivePath, string EntryPath)
    : AttributedShellMultiScript<CatLine>(ArchivePath, EntryPath);