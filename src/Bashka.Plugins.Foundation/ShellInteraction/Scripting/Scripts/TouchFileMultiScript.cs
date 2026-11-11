namespace Bashka.Plugins.Foundation.ShellInteraction.Scripting.Scripts;

[ShellScript(typeof(BashExecutor), """touch ""{0}"" """)]
public record TouchFileMultiScript(string FullPath)
    : AttributedShellMultiScript(FullPath);