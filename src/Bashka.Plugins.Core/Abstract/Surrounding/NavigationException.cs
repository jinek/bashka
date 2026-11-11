using System.Runtime.Serialization;

namespace Bashka.Plugins.Core.Abstract.Surrounding;

public class NavigationException : Exception
{
    public NavigationException()
    {
    }

    protected NavigationException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }

    public NavigationException(string? message) : base(message)
    {
    }

    public NavigationException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}