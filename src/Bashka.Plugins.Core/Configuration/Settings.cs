using System.ComponentModel;
using System.Reflection;
using Bashka.Helpers;
using IniParser.Model;
using IniParser.Parser;

namespace Bashka.Plugins.Core.Configuration;

/// <summary>
///     Merged once at startup, later wins key by key: ini files of plugins in registration order, user ini files in name
///     order, state. An empty value unsets the key. Changes of files apply after restart.
///     Every file has a single owner: Bashka writes only the state file.
/// </summary>
public sealed class Settings
{
    public static readonly string UserDirectory = Path.Combine(BashkaHelpers.DataDirectory, "Settings");
    public static readonly string StateFilePath = Path.Combine(BashkaHelpers.DataDirectory, "state.ini");

    private static readonly StringComparer Comparer = StringComparer.OrdinalIgnoreCase;
    private readonly Lock _stateLock = new();
    private readonly Dictionary<string, Dictionary<string, string>> _values = new(Comparer);

    public Settings(IEnumerable<PluginSettingsFile> defaultPluginFiles)
    {
        PluginFiles = [.. defaultPluginFiles];

        foreach (string text in PluginFiles.Select(file => file.Content))
            Merge(Parse(text));
        foreach (string userFile in UserFiles)
            Merge(Read(userFile));
        Merge(Read(StateFilePath));
    }

    public IReadOnlyList<PluginSettingsFile> PluginFiles { get; }

    public static IEnumerable<string> UserFiles => Directory.Exists(UserDirectory)
        ? Directory.EnumerateFiles(UserDirectory, "*.ini",
                new EnumerationOptions
                {
                    MatchCasing = MatchCasing.CaseInsensitive,
                    AttributesToSkip = 0
                })
            .Order(StringComparer.Ordinal)
        : [];

    public static T Bind<T>(Settings settings) where T : class, new()
    {
        string section = typeof(T).GetCustomAttribute<SettingsSectionAttribute>()?.Name ?? typeof(T).Name;
        var instance = new T();
        PropertyInfo[] properties =
        [
            .. typeof(T).GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .Where(property => property is { CanRead: true, CanWrite: true })
        ];

        foreach (PropertyInfo property in properties)
            if (settings.Get(section, property.Name) is { } value)
                property.SetValue(instance, Converter(property).ConvertFromInvariantString(value));

        if (instance is INotifyPropertyChanged notifying)
            notifying.PropertyChanged += (_, args) =>
            {
                PropertyInfo? property = properties.SingleOrDefault(item => item.Name == args.PropertyName);
                if (property == null) return;
                settings.SetState(section, property.Name,
                    Converter(property).ConvertToInvariantString(property.GetValue(instance)) ?? string.Empty);
            };

        return instance;

        static TypeConverter Converter(PropertyInfo property)
        {
            return TypeDescriptor.GetConverter(property.PropertyType);
        }
    }

    private string? Get(string section, string key)
    {
        return _values.GetValueOrDefault(section)?.GetValueOrDefault(key);
    }

    /// <summary>
    ///     The state file is read right before writing, so keys written by others survive
    /// </summary>
    private void SetState(string section, string key, string value)
    {
        lock (_stateLock)
        {
            IniData state = Read(StateFilePath);
            if (state[section][key] == value) return;
            state[section][key] = value;
            Directory.CreateDirectory(Path.GetDirectoryName(StateFilePath)!);
            string temporary = StateFilePath + ".tmp";
            File.WriteAllText(temporary, state.ToString());
            File.Move(temporary, StateFilePath, true);
            Merge(state);
        }
    }

    private void Merge(IniData layer)
    {
        foreach (SectionData section in layer.Sections)
        {
            if (!_values.TryGetValue(section.SectionName, out Dictionary<string, string>? values))
                _values[section.SectionName] = values = new Dictionary<string, string>(Comparer);
            foreach (KeyData keyData in section.Keys)
                if (string.IsNullOrEmpty(keyData.Value)) values.Remove(keyData.KeyName);
                else values[keyData.KeyName] = keyData.Value;
        }
    }

    private static IniData Read(string path)
    {
        return File.Exists(path) ? Parse(File.ReadAllText(path)) : new IniData();
    }

    private static IniData Parse(string text)
    {
        var parser = new IniDataParser
        {
            Configuration =
            {
                AllowDuplicateKeys = true,
                OverrideDuplicateKeys = true,
                AllowDuplicateSections = true,
                CaseInsensitive = true
            }
        };
        return parser.Parse(text);
    }
}