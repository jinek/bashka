namespace Bashka.Plugins.Foundation.FileSystem;

public class RenameCommand : MoveCommand
{
    protected override bool ToAnotherPanel => false;

    protected override string DialogTitle => "Enter new name";
}
