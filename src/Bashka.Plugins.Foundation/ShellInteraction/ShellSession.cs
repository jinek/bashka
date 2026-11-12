using System.Reflection;
using System.Text.RegularExpressions;
using Bashka.Plugins.Core.Abstract;
using Bashka.Plugins.Core.Scoping;
using Bashka.Plugins.Foundation.ShellInteraction.Scripting;
using Bashka.Plugins.Foundation.ShellInteraction.Scripting.Scripts;

namespace Bashka.Plugins.Foundation.ShellInteraction;

public sealed class ShellSession : BaseObject, IDisposable
{
    // todo: all this logic must be rewritten
    private readonly IEnumerable<IScriptExecutor> _allExecutors;//todo: all this stuff really can work only when one executor registered
    public IShellConnection ShellConnection { get; }

    public ShellSession(/*IComponentContext componentContext, todo: delete this parameter*/IEnumerable<IScriptExecutor> allExecutors, IShellConnection shellConnection)
    {
        _allExecutors = allExecutors;
        ShellConnection = shellConnection;
    }

    /// <summary>
    /// Null if not determined yet. You can enforce it by calling <see cref="DetermineShell"/>
    /// </summary>
    public IReadOnlyList<IScriptExecutor>? AvailableBackends { get; private set; }

    public T ExecuteScalar<T>(AttributedShellMultiScript<T> shellMultiScript)
    {
        return PrepareQuery(shellMultiScript).Single();
    }
    
    public void ExecuteNonQuery(ShellMultiScript shellMultiScript)
    {
        object[] _ = PrepareQuery(shellMultiScript).ToArray();
    }

    public IEnumerable<T> PrepareQuery<T>(AttributedShellMultiScript<T> shellMultiScript)
    {
        return PrepareQuery((ShellMultiScript)shellMultiScript).Cast<T>();
    }
    
//todo:  session initialization needs sync:    [MethodImpl(MethodImplOptions.Synchronized)]
    public IEnumerable<object> PrepareQuery(ShellMultiScript shellMultiScript)
    {
        if (AvailableBackends == null)
        {
            DetermineShell();
            InitializeShellSession();
        }
        
        //todo: try this to see black magic: Thread.Sleep(new Random(Environment.TickCount).Next(10000));//todo: remove

        ShellScriptAttribute shellScript =
            ShellHelper.BestMatch(AvailableBackends!, shellMultiScript, out IScriptExecutor executor);

        IEnumerable<string> resultStrings = executor.PrepareScriptExecution(ShellConnection,
            string.Format(shellScript.ScriptFormat, shellMultiScript.Arguments));

        if (shellScript.ParseExpression != null)
        {
            var regex = new Regex(shellScript.ParseExpression,RegexOptions.Singleline/*RegexOptions.Multiline todo: allow buffering in one string also*/);
            ConstructorInfo constructorInfo = shellMultiScript.ResultType.GetConstructors().Single();
            ParameterInfo[] parameterInfos = constructorInfo.GetParameters();

            foreach (string resultString in resultStrings)
            {
                Match match = regex.Match(resultString);
                if (!match.Success) continue;

                object?[] parameters = parameterInfos.Select(parameterInfo =>
                {
                    //todo: also can be primitive types
                    return parameterInfo.ParameterType.IsArray
                        ? (object)match.Groups[parameterInfo.Name!].Captures.Select(capture => capture.Value).ToArray()
                        : match.Groups[parameterInfo.Name!].Value;
                }).ToArray();

                yield return constructorInfo.Invoke(parameters);                
            }
        }
        else
        {
            // Case when no parsing needed+
            foreach (string resultString in resultStrings)
            {
                yield return resultString;
            }
        }
    }

    private void InitializeShellSession()
    {
        ExecuteNonQuery(new EchoShellMultiScript("CCV Navigator connected")); //reading welcome message
    }

    public void DetermineShell()
    {
        AvailableBackends = _allExecutors.Where(executor => executor.CanExecute).ToArray();
    }

    public void Dispose()
    {
        //todo: throw new NotImplementedException();
    }
}