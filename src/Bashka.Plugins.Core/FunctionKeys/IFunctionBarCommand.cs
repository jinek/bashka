using Bashka.Plugins.Core.Abstract;
using Bashka.Plugins.Core.Abstract.Data;

namespace Bashka.Plugins.Core.FunctionKeys;

public interface IFunctionBarCommand<in TCatalog /*invariance used by reflection of ftoolbox*/> : ICommand
    where TCatalog : ICatalog
{
    const string DefaultTitleMetadataKey = "DefaultTitle";
    const string IsDeleteMetadataKey = "IsDelete";


    /// <summary>
    ///     By default detecting by catalog name
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    bool IsAllowed(CommandExecutionContext context)
    {
        return context.Catalog is TCatalog;
    }
}