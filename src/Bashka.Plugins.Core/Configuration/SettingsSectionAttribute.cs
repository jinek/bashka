namespace Bashka.Plugins.Core.Configuration;

[AttributeUsage(AttributeTargets.Class)]
public sealed class SettingsSectionAttribute(string name) : Attribute
{
    public string Name { get; } = name;
}