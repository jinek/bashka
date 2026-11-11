namespace Bashka.Plugins.Core.Scoping
{
    public interface ITypeKeyedObject
    {
        string TypeKey => GetType().GetTypeKey();
    }
}