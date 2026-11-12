namespace Bashka.Plugins.Core.Abstract;

public interface IScriptExecutor
{
    bool CanExecute { get; }
    IEnumerable<string> PrepareScriptExecution(IShellConnection shellConnection, string script);
    bool CanExecuteBackend(Type scriptExecutorType);
}