using System.Diagnostics;
using Bashka.Plugins.Core.Abstract;
using Renci.SshNet;

namespace Bashka.Plugins.Foundation.ShellInteraction
{
    class SshShellConnection : IShellConnection
    {
        private readonly ConnectionInfo _connectionInfo;
        private readonly SshClient _sshClient;
        private  readonly ShellStream _shellStream;
        private readonly object _disposableLock = new();


        public SshShellConnection(ConnectionInfo connectionInfo)
        {
            _connectionInfo = connectionInfo;
            _sshClient = new SshClient(_connectionInfo);
            _sshClient.Connect();
            _shellStream = _sshClient.CreateShellStream("xterm", 80, 24, 800, 600, 1024);
        }

        public IEnumerable<string> PrepareExecution(string command, string parameter)
        {
            string input = command + " " + parameter;

            lock (_disposableLock)
            {
                Debug.WriteLine("> " + input);
                _shellStream.WriteLine(input);

                string? line;

                while ((line = _shellStream.ReadLine()) != null)
                {
                    Debug.WriteLine("< " + line);
                    yield return line;
                }

                Debug.WriteLine("<~ " + line);
            }
        }

        public string Title => _connectionInfo.Host+":"+_connectionInfo.Port;
        public byte MessageIdHitCount => 2;
        public bool IsEquivalentTo(object another)
        {
            return another is SshShellConnection anotherSshShellConnection
                   && anotherSshShellConnection._connectionInfo.Host ==
                   _connectionInfo.Host; // different users can have different paths
        }
    }
}