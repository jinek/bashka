using Bashka.Plugins.Foundation.ShellInteraction.Scripting;
using Bashka.Plugins.Foundation.ShellInteraction.Scripting.Scripts;

namespace Bashka.Plugins.Foundation.FileSystem;

public class CopyCommand : CopyOrMoveCommandBase
{
    protected override string DialogTitle => "Enter file name";

    protected override ShellMultiScript CreateScript(string sourcePath, string destinationPath)
    {
        return new CopyFileMultiScript(sourcePath, destinationPath);
    }
}