using Bashka.Plugins.Core.Dialog.Abstract;

namespace Bashka.Plugins.Foundation;

public class HelloDialogContent : DialogContent
{
    public HelloDialogContent() : base("Sample dialog", "centered", "min_size")
    {
    }

    public string Message { get; set; } = "Hello Dialog!";
}