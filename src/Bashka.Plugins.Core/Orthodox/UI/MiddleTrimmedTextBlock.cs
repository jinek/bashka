using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.TextFormatting;
using PropertyChanged;

namespace Bashka.Plugins.Core.Orthodox.UI;

/// <summary>
///     Shows <see cref="FullText" /> cut in the middle with <see cref="Ellipsis" /> when there is not enough space
/// </summary>
[DoNotNotify]
public class MiddleTrimmedTextBlock : TextBlock
{
    //vibecoded!
    private const string Ellipsis = "..";

    public static readonly StyledProperty<string?> FullTextProperty =
        AvaloniaProperty.Register<MiddleTrimmedTextBlock, string?>(nameof(FullText));

    static MiddleTrimmedTextBlock()
    {
        AffectsMeasure<MiddleTrimmedTextBlock>(FullTextProperty);
    }

    public string? FullText
    {
        get => GetValue(FullTextProperty);
        set => SetValue(FullTextProperty, value);
    }

    protected override Type StyleKeyOverride => typeof(TextBlock);

    protected override Size MeasureOverride(Size availableSize)
    {
        string trimmed = Trim(FullText ?? string.Empty, availableSize.Deflate(Padding).Width);
        if (Text != trimmed)
            SetCurrentValue(TextProperty, trimmed);
        return base.MeasureOverride(availableSize);
    }

    private string Trim(string text, double width)
    {
        var typeface = new Typeface(FontFamily, FontStyle, FontWeight, FontStretch);
        if (double.IsInfinity(width) || Fits(text))
            return text;

        string[] elements = TextElements(text);
        int low = 0, high = elements.Length - 1;
        while (low < high)
        {
            int middle = (low + high + 1) / 2;
            if (Fits(Join(middle))) low = middle;
            else high = middle - 1;
        }

        return Fits(Join(low)) ? Join(low) : Ellipsis[..Math.Clamp((int)width, 0, Ellipsis.Length)];

        bool Fits(string candidate)
        {
            using var layout = new TextLayout(candidate, typeface, FontSize);
            return layout.WidthIncludingTrailingWhitespace <= width;
        }

        string Join(int kept)
        {
            int head = (kept + 1) / 2;
            return string.Concat(elements[..head]) + Ellipsis + string.Concat(elements[^(kept - head)..]);
        }
    }

    private static string[] TextElements(string text)
    {
        var result = new List<string>();
        TextElementEnumerator enumerator = StringInfo.GetTextElementEnumerator(text);
        while (enumerator.MoveNext())
            result.Add(enumerator.GetTextElement());
        return [.. result];
    }
}