namespace Bashka.Plugins.Core.Abstract.Surrounding;

public class UserErrorException : Exception
{
    public UserErrorException(string message) : base(message)
    {
    }
}