namespace Bashka.Plugins.Core.Abstract.Surrounding;

public class InvalidPathException : Exception
{
    public InvalidPathException(string? currentPath) :
        base($"Invalid path {currentPath}")
    {
        throw new NotImplementedException();
    }
}