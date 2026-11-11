namespace Bashka.Plugins.Foundation.ShellInteraction.Scripting.Scripts;

[ShellScript(typeof(BashExecutor), """mkdir ""{0}"" """)]
public record MkdirMultiScript(string FullPath)
    : AttributedShellMultiScript(FullPath);