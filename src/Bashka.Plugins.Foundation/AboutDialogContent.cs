using Bashka.Plugins.Core.Dialog.Abstract;

namespace Bashka.Plugins.Foundation;

public class AboutDialogContent : DialogContent
{
    public AboutDialogContent() : base("About Bashka", "centered", "min_size")
    {
    }

    public string Version { get; } = typeof(AboutDialogContent).Assembly.GetName().Version!.ToString(4);

    public void Close()
    {
        RaiseClose();
    }
}