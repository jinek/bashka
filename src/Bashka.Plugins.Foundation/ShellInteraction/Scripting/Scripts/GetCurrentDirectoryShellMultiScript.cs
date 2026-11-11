namespace Bashka.Plugins.Foundation.ShellInteraction.Scripting.Scripts;

[ShellScript(typeof(BashExecutor), "pwd", "^(?<Line>/.*)$")]
public record GetCurrentDirectoryShellMultiScript
    : AttributedShellMultiScript<CurrentDirectoryLine>;
