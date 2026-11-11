using Bashka.Plugins.Foundation.ShellInteraction.Scripting;
using Bashka.Plugins.Foundation.ShellInteraction.Scripting.Scripts;

namespace Bashka.Plugins.Foundation.FileSystem;

public class MoveCommand : CopyOrMoveCommandBase
{
    protected override string DialogTitle => "Enter file name";

    protected override bool SourceIsGone => ToAnotherPanel;

    protected override ShellMultiScript CreateScript(string sourcePath, string destinationPath)
    {
        return new MoveFileMultiScript(sourcePath, destinationPath);
    }
}
