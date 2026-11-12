namespace Bashka.Plugins.Core.Abstract;

public interface IShellConnection : IEquivalent
{
    const string DefaultShellConnectionKey = "default-shell";
    string Title { get; }
    byte MessageIdHitCount { get; }
    IEnumerable<string> PrepareExecution(string command, string parameter);
}