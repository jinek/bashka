using Bashka.Plugins.Core.Abstract;
using Bashka.Plugins.Foundation.ShellInteraction.Scripting;

namespace Bashka.Plugins.Foundation.ShellInteraction;

public static class ShellHelper
{
    public static ShellScriptAttribute BestMatch(IScriptExecutor[] availableBackends,
        ShellMultiScript shellMultiScript, out IScriptExecutor scriptExecutor)
    {
        foreach (ShellScriptAttribute script in shellMultiScript.Scripts)
        foreach (IScriptExecutor executor in availableBackends)
        {
            if (!executor.CanExecuteBackend(script.ExecutorType)) //todo: copypaste 
                continue;

            scriptExecutor = executor;
            return script;
        }

        throw new NotImplementedException("What to do if no executors for this command?");
    }
}