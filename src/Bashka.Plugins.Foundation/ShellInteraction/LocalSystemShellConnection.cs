using System.Diagnostics;
using Bashka.Plugins.Core.Abstract;
using Bashka.Plugins.Core.Scoping;

namespace Bashka.Plugins.Foundation.ShellInteraction;

public class LocalSystemShellConnection : BaseObject, IShellConnection, IDisposable
{
    private readonly object _lock = new();
    private readonly Process _shellProcess;

    public LocalSystemShellConnection()
    {
        _shellProcess = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "/bin/bash", // for Linux and MacOS
                // FileName = "cmd.exe", // for Windows //todo: 
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };

        _shellProcess.Start();
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    public IEnumerable<string> PrepareExecution(string command, string parameter)
    {
        lock (_lock)
        {
            string buffer = command + " " + parameter;
            //Debug.WriteLine("> " + buffer);
            _shellProcess.StandardInput.WriteLine(buffer);

            while (_shellProcess.StandardOutput.ReadLine() is { } line)
                //Debug.WriteLine("< " + line);
                yield return line;
        }
    }

    public string Title => "▸Local system◂";

    public byte MessageIdHitCount => 1;

    public bool IsEquivalentTo(object another)
    {
        return another is LocalSystemShellConnection;
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
            lock (_lock)
            {
                _shellProcess.Dispose();
            }
    }
}