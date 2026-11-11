namespace Bashka.Plugins.Foundation.ShellInteraction.Scripting.Scripts;

[ShellScript(typeof(BashExecutor), """cat "{0}"; echo""", @"^(?<Line>.+)$")]
public record CatShellMultiScript(string FullPath)
    : AttributedShellMultiScript<CatLine>(FullPath);