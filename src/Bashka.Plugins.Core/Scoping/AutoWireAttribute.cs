namespace Bashka.Plugins.Core.Scoping;

[AttributeUsage(AttributeTargets.Property)]
public class AutoWireAttribute : Attribute
{
    public object? Key { get; set; }
}