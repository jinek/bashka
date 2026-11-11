namespace Bashka.Plugins.Foundation.ShellInteraction.Scripting.Scripts;

[ShellScript(typeof(BashExecutor), """cp ""{0}"" ""{1}"" """)]
public record CopyFileMultiScript(string SourcePath, string DestinationPath)
    : AttributedShellMultiScript(SourcePath, DestinationPath);