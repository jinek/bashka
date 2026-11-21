using System.Reflection;

namespace Bashka.Plugins.Core.Orthodox;

/// <summary>
///     Every public string property binds the command keyed by the property name to comma separated key combinations.
///     Plugins bring bindings for own commands by inheriting and registering the inheritor instead
/// </summary>
public abstract class KeyBindings
{
    private ILookup<KeyCombination, string>? _commandNames;

    public IEnumerable<string> MatchCommandNames(KeyCombination keyCombination)
    {
        _commandNames ??= GetType()
            .GetProperties(BindingFlags.Instance | BindingFlags.Public)
            .Where(property => property.PropertyType == typeof(string))
            .SelectMany(property => ((string?)property.GetValue(this) ?? string.Empty)
                .Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
                .Select(keys => (KeyCombination: new KeyCombination(keys), CommandName: property.Name)))
            .ToLookup(binding => binding.KeyCombination, binding => binding.CommandName);
        return _commandNames[keyCombination];
    }
}