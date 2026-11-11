namespace Bashka.Plugins.Foundation.ShellInteraction.Scripting.Scripts;

[ShellScript(typeof(BashExecutor),
    @"ls -lA --color=never --full-time --group-directories-first --indicator-style=none --show-control-chars --quote-name ""{0}""",
    @"^total\s+(?<Size>\d+)$|(?<Type>[bcdlpsD-])(?<Permission>[rwxsStT-]{9})[.+@]?\s+\d+\s+(?<Owner>\S+)\s+(?<Group>\S+)\s+(?<Size>\d+)\s+(?<TimeStamp>\d{4}-\d{2}-\d{2}[ T]\d{2}:\d{2}:\d{2}(?:\.\d+)?\s*[+-]\d{4})\s+""(?<Name>.+)""$")]
public record DirShellMultiScript(string FullPath)
    : AttributedShellMultiScript
        <RawFile>(FullPath);