namespace Bashka.Plugins.Foundation.ShellInteraction.Scripting.Scripts;

[ShellScript(typeof(BashExecutor), @"cd ""{0}""")]
public record ChangeDirectoryShellMultiScript(string Path)
    : AttributedShellMultiScript(Path);