using Bashka.Plugins.Foundation.ShellInteraction;
using Bashka.Plugins.Foundation.ShellInteraction.Scripting;

namespace Bashka.Plugins.Archives.Bsdtar.Scripts;

[ShellScript(typeof(BashExecutor), """LC_ALL=C bsdtar -tvf "{0}" """,
    @"^(?<Type>.)(?<Permissions>[rwxstST-]{9})\S*\s+\S+\s+\S+\s+\S+\s+(?<Size>\d+)\s+(?<Month>\S+)\s+(?<Day>\S+)\s+(?<TimeOrYear>\S+)\s+(?<Name>.+)$")]
public record BsdtarListShellMultiScript(string ArchivePath)
    : AttributedShellMultiScript<RawBsdtarEntry>(ArchivePath);