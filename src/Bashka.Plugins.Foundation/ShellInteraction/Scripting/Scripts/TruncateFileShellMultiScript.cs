namespace Bashka.Plugins.Foundation.ShellInteraction.Scripting.Scripts;

[ShellScript(typeof(BashExecutor), """: > "{0}" """)]
public record TruncateFileShellMultiScript(string FullPath)
    : AttributedShellMultiScript(FullPath);