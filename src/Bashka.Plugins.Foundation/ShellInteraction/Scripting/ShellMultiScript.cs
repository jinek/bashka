using System.Collections.ObjectModel;

namespace Bashka.Plugins.Foundation.ShellInteraction.Scripting;

public record ShellMultiScript
{
    protected ShellMultiScript(Type resultType, params object[] arguments)
    {
        ResultType = resultType;
        Arguments = arguments;
    }

    public Collection<ShellScriptAttribute> Scripts { get; } = [];
    public IReadOnlyCollection<object> Arguments { get; }
    public Type ResultType { get; }
}