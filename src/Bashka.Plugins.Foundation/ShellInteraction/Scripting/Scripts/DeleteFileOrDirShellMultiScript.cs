namespace Bashka.Plugins.Foundation.ShellInteraction.Scripting.Scripts;

[ShellScript(typeof(BashExecutor), """rm {0} -- '{1}'""")]
public record DeleteFileOrDirShellMultiScript(bool IsDirectory, string FullPath) : AttributedShellMultiScript(
    IsDirectory ? "-r " : "",
    FullPath.Replace("'", "'\"'\"'"));