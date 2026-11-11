using JetBrains.Annotations;

namespace Bashka.Plugins.Foundation.ShellInteraction.Scripting.Scripts;

[UsedImplicitly]
public record RawFile(
    string Type,
    string Permission,
    string Owner,
    string Group,
    string Size,
    string TimeStamp,
    string Name);