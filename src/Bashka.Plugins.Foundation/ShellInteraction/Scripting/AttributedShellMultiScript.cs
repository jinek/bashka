using System.Reflection;
using DynamicData;

namespace Bashka.Plugins.Foundation.ShellInteraction.Scripting;

public record AttributedShellMultiScript(params IReadOnlyCollection<object> Arguments)
    : AttributedShellMultiScript<object>([.. Arguments]);

public record AttributedShellMultiScript<TResult> : ShellMultiScript
{
    protected AttributedShellMultiScript(params object[] arguments) : base(typeof(TResult), arguments)
    {
        Scripts.AddRange(GetType().GetCustomAttributes<ShellScriptAttribute>());
    }
}