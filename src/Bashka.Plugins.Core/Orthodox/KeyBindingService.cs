using Autofac.Features.Indexed;
using Bashka.Plugins.Core.Abstract;
using IniParser;
using IniParser.Model;

namespace Bashka.Plugins.Core.Orthodox;

public class KeyBindingService
{
    private readonly IIndex<string, ICommand> _commandFactory;
    private readonly Dictionary<(string scope, KeyCombination keyCombination), string[]> _keyBindings = new();
    private const string AltCharActivatesSearchKey = "AltCharActivatesSearch";
    private const string KeysSuffix = "(keys)";

    public bool AltCharActivatesSearch { get; }


    public KeyBindingService(IIndex<string, ICommand> commandFactory)
    {
        _commandFactory = commandFactory;
        var fileIniDataParser = new FileIniDataParser();
        IniData bindingsData = fileIniDataParser.ReadFile(Path.Combine(AppContext.BaseDirectory, "keybindings.ini"));
        
        // Process sections
        foreach (SectionData sectionData in bindingsData.Sections)
        {
            string sectionName = sectionData.SectionName;
            if (sectionName.EndsWith(KeysSuffix, StringComparison.OrdinalIgnoreCase))
            {
                sectionName = sectionName[..^KeysSuffix.Length];
                ProcessKeyData(sectionName, sectionData.Keys);
            }
        }
        // todo: many things I don't like in this service. It's bound to Orthodox concept. Should be inverted
        AltCharActivatesSearch = bool.TryParse(bindingsData[nameof(OrthodoxCommander)][AltCharActivatesSearchKey], out bool altCharActivatesSearch) && altCharActivatesSearch;
        bindingsData.Global.RemoveKey(AltCharActivatesSearchKey);

        // Process global bindings
        ProcessKeyData(string.Empty, bindingsData.Global);
       
        return;
        
        void ProcessKeyData(string scope, KeyDataCollection keyDataCollection)
        {
            foreach (KeyData keyData in keyDataCollection)
            {
                string keyCombinationString = keyData.KeyName;
                KeyCombination keyCombination = new KeyCombination(keyCombinationString);
                string[] commandNames = keyData.Value.Split(',',
                    StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

                _keyBindings.Add((scope, keyCombination), commandNames);
            }
        }
    }

    public ICommand[] MatchCommands(KeyCombination keyCombination, params string[] scopes)
    {
        if (scopes.Contains(string.Empty))
            throw new InvalidOperationException("Empty scope is not allowed. It will be used automatically");
        scopes = scopes.Append(string.Empty).ToArray();

        var commandNames = new List<string>();
        foreach (string scope in scopes)
        {
            if (_keyBindings.TryGetValue((scope, keyCombination), out string[]? names))
            {
                commandNames.AddRange(names);
            }
        }

        return commandNames.Select(commandName => _commandFactory[commandName]).ToArray();
    }
}
