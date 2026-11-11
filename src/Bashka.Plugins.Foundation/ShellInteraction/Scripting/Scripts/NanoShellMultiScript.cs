namespace Bashka.Plugins.Foundation.ShellInteraction.Scripting.Scripts;

[ShellScript(typeof(BashExecutor), """
                                   nano "{0}"
                                   """)]
public record NanoShellMultiScript(string FullPath) : AttributedShellMultiScript(FullPath);