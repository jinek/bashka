using Autofac;
using Bashka.Plugins.Core.Abstract;
using JetBrains.Annotations;

namespace Bashka.Plugins.Foundation.ShellInteraction.Scripting;

[AttributeUsage(AttributeTargets.Class)]
public sealed class ShellScriptAttribute : Attribute
{
    public ShellScriptAttribute(Type executorType, string scriptFormat, [RegexPattern] string? parseExpression = null)
    {
        if (!executorType.IsAssignableTo<IScriptExecutor>())
            throw new ArgumentOutOfRangeException(nameof(executorType));
        ExecutorType = executorType;
        ScriptFormat = scriptFormat;
        ParseExpression = parseExpression;
    }

    public Type ExecutorType { get; }
    public string ScriptFormat { get; }
    public string? ParseExpression { get; }
}