using Bashka.Plugins.Core.Abstract.Surrounding;
using Bashka.Plugins.Core.Scoping;
using JetBrains.Annotations;

namespace Bashka.Plugins.Foundation.ShellInteraction;

public abstract class ShellCatalog : PathCatalog
{
    [AutoWire]
    public ShellSession
        ShellSession { get; [UsedImplicitly] internal set; } = null!; // почему то сюда не попадает дефолтовый шел
}