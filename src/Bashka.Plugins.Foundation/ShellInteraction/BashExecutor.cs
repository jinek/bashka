using Bashka.Plugins.Core.Abstract;

namespace Bashka.Plugins.Foundation.ShellInteraction;

public class BashExecutor : IScriptExecutor
{
    public const string BashKey = "bash"; 
    public IEnumerable<string> PrepareScriptExecution(IShellConnection shellConnection, string script)
    {
        string commandId = Guid.NewGuid().ToString();
        string input = script + @" ; echo '" + commandId + @"'";

        IEnumerable<string> enumerable = shellConnection.PrepareExecution(input, string.Empty);
        
        byte hit = 0;
        foreach (string line in enumerable)
        {
            if (line.Contains(commandId))
            {
                hit++;
                if(hit >= shellConnection.MessageIdHitCount)
                    yield break;
            }
            else
            {
                yield return line;
            }
        }
    }

    public bool CanExecute => true;//todo: check if bash is here
    public bool CanExecuteBackend(Type scriptExecutorType) => GetType().IsAssignableTo(scriptExecutorType);
}