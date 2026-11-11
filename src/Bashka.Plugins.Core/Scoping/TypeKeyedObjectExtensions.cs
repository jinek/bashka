namespace Bashka.Plugins.Core.Scoping
{
    public static class TypeKeyedObjectExtensions
    {
        public static string GetTypeKey(this Type type)
        {
            return type.FullName!;
        }
    }
}