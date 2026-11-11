namespace Bashka.Plugins.Foundation.ShellInteraction.Scripting.Scripts;

[ShellScript(typeof(BashExecutor), """base64 "{0}"; echo""", @"^\s*(?<Line>[A-Za-z0-9+/=]+)\s*$")]
public record ReadBase64ShellMultiScript(string FullPath)
    : AttributedShellMultiScript<Base64Line>(FullPath);
