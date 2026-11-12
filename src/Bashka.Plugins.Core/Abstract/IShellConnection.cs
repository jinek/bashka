namespace Bashka.Plugins.Core.Abstract;

public interface IShellConnection : IEquivalent
{
    const string DefaultShellConnectionKey = "default-shell"; 
    IEnumerable<string> PrepareExecution(string command, string parameter);
    public string Title { get; }
    public byte MessageIdHitCount { get; }
}