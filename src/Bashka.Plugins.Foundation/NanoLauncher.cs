using Bashka.Plugins.Foundation.ShellInteraction;

namespace Bashka.Plugins.Foundation;

public static class NanoLauncher
{
    public static Task OpenAsync(IPtyExecutor catalog, string workingDir, string path)
    {
        return catalog.ExecuteAsResidentAsync(workingDir, "nano", path, CancellationToken.None);
    }
}
