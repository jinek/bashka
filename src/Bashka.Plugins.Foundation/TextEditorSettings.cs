using Bashka.Plugins.Core.Configuration;

namespace Bashka.Plugins.Foundation;

[SettingsSection("TextEditor")]
public class TextEditorSettings
{
    public const string AutoEditor = "auto";

    public string Editor { get; init; } = AutoEditor;
}