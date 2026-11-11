namespace Bashka.Plugins.Foundation.ShellInteraction.Scripting;

public record ShellMultiScript(Type ResultType, params object[] Arguments)
{
    public List<ShellScriptAttribute> Scripts { get; } = new();
}