namespace Bashka.Plugins.Foundation.ShellInteraction.Scripting.Scripts;

[ShellScript(typeof(BashExecutor), """mv ""{0}"" ""{1}"" """)]
public record MoveFileMultiScript(string SourcePath, string DestinationPath)
    : AttributedShellMultiScript(SourcePath, DestinationPath);
