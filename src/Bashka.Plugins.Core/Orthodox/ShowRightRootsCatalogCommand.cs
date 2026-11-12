using JetBrains.Annotations;

namespace Bashka.Plugins.Core.Orthodox;

[UsedImplicitly]
public sealed class ShowRightRootsCatalogCommand : ShowLeftRootsCatalogCommand
{
    public ShowRightRootsCatalogCommand()
    {
        LeftToActivate = false;
        RightToActivate = true;
    }
}