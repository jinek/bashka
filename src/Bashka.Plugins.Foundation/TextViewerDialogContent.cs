using Bashka.Plugins.Core.Dialog.Abstract;

namespace Bashka.Plugins.Foundation;

public class TextViewerDialogContent : DialogContent
{
    public TextViewerDialogContent(string fileName, string text) : base($"{fileName}", "small_margin")
    {
        Text = text;
    }

    public string Text { get; }
}