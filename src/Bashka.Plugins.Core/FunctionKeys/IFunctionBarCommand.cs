using Bashka.Plugins.Core.Abstract;
using Bashka.Plugins.Core.Abstract.Data;

namespace Bashka.Plugins.Core.FunctionKeys
{
    /// <summary>
    /// <see cref="DefaultFSlotAttribute"/>
    /// </summary>
    public interface IFunctionBarCommand<in TCatalog/*invariance used by reflection of ftoolbox*/> : ICommand where TCatalog : ICatalog
    {
        public const string DefaultTitleMetadataKey = "DefaultTitle";
        public const string IsDeleteMetadataKey = "IsDelete";
    
    
        /// <summary>
        /// By default detecting by catalog name
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public bool IsAllowed(CommandExecutionContext context)
        {
            if (context.Catalog is not TCatalog) return false;
        
            return true;
        }
    }
}