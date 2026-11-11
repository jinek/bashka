namespace Bashka.Plugins.Foundation.ShellInteraction.Scripting.Scripts;

[ShellScript(typeof(BashExecutor), "xdg-open '{0}'")]
public record ShellExecuteMultiScript(string fileNameWithExtension) : AttributedShellMultiScript(fileNameWithExtension);