using Bashka.Plugins.Core.Abstract;

namespace Bashka.Plugins.Foundation.ShellInteraction
{
    public interface IPtyExecutor : ICatalog
    {
        Task ExecuteAsResidentAsync(string workingDir, string applicationName, string arguments, CancellationToken cancellationToken);
    }
}