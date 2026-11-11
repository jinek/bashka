namespace Bashka.Plugins.Foundation.ShellInteraction.Scripting.Scripts;

[ShellScript(typeof(BashExecutor), @"echo ""{0}""")]
public record EchoShellMultiScript(string Text)
    : AttributedShellMultiScript(Text);