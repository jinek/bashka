namespace Bashka.Plugins.Core.Abstract;

public interface IScriptExecutor
{
    public IEnumerable<string> PrepareScriptExecution(IShellConnection shellConnection, string script);
    bool CanExecute { get; }
    bool CanExecuteBackend(Type scriptExecutorType);
}