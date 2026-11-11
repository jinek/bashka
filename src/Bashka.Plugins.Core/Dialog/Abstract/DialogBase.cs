using Bashka.Plugins.Core.Scoping;

namespace Bashka.Plugins.Core.Dialog.Abstract;

public class DialogBase : BaseObject
{
    public bool IsOkEnabled { get; protected set; }
    public bool IsOkVisible { get; protected set; } = true;
    public string CancelText { get; protected set; } = "Cancel";
}