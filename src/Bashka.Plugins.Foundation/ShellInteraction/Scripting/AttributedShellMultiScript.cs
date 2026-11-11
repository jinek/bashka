using System.Reflection;

namespace Bashka.Plugins.Foundation.ShellInteraction.Scripting;

public record AttributedShellMultiScript(params object[] Arguments) : AttributedShellMultiScript<object>(Arguments);

public record AttributedShellMultiScript<TResult> : ShellMultiScript
{
    protected AttributedShellMultiScript(params object[] arguments) : base(typeof(TResult), arguments)
    {
        Scripts.AddRange(GetType().GetCustomAttributes<ShellScriptAttribute>());
    }
}