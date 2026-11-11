using JetBrains.Annotations;

namespace Bashka.Plugins.Archives.Bsdtar.Scripts;

[UsedImplicitly]
public record RawBsdtarEntry(
    string Type,
    string Permissions,
    string Size,
    string Month,
    string Day,
    string TimeOrYear,
    string Name);