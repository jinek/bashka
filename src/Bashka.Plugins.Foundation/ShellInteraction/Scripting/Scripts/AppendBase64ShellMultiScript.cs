namespace Bashka.Plugins.Foundation.ShellInteraction.Scripting.Scripts;

[ShellScript(typeof(BashExecutor), """printf '%s' '{1}' | base64 -d >> "{0}" """)]
public record AppendBase64ShellMultiScript(string FullPath, string Base64Chunk)
    : AttributedShellMultiScript(FullPath, Base64Chunk);
