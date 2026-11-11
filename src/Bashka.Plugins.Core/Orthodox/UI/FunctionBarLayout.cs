using Avalonia;
using Avalonia.Layout;
using PropertyChanged;

namespace Bashka.Plugins.Core.Orthodox.UI;

[DoNotNotify]
public class FunctionBarLayout : NonVirtualizingLayout
{//todo: this was rewritten by Cluade. I'm sure there is 5 kg of copypaste and 3 of shitcode here
    public static readonly StyledProperty<int> GapProperty =
        AvaloniaProperty.Register<FunctionBarLayout, int>(nameof(Gap));

    public int Gap
    {
        get => GetValue(GapProperty);
        set => SetValue(GapProperty, value);
    }

    protected override Size MeasureOverride(NonVirtualizingLayoutContext context, Size availableSize)
    {
        var children = context.Children;
        int count = children.Count;
        if (count == 0)
        {
            return new Size(0, 0);
        }

        double maxChildHeight = 0;
        double totalDesiredWidth = 0;
        var childAvailableSize = new Size(double.PositiveInfinity, availableSize.Height);

        for (int i = 0; i < count; i++)
        {
            var child = children[i];
            child.Measure(childAvailableSize);
            maxChildHeight = Math.Max(maxChildHeight, child.DesiredSize.Height);
            totalDesiredWidth += child.DesiredSize.Width;
        }

        double width = double.IsInfinity(availableSize.Width)
            ? totalDesiredWidth + Gap * (count - 1)
            : availableSize.Width;

        double height = double.IsInfinity(availableSize.Height)
            ? maxChildHeight
            : availableSize.Height;

        return new Size(width, height);
    }

    protected override Size ArrangeOverride(NonVirtualizingLayoutContext context, Size finalSize)
    {
        var children = context.Children;
        int count = children.Count;
        if (count == 0)
        {
            return new Size(0, 0);
        }

        double width = finalSize.Width;
        double height = finalSize.Height;

        double availableWidthForColumns = width - Gap * (count - 1);
        if (availableWidthForColumns <= 0)
        {
            for (int i = 0; i < count; i++)
            {
                children[i].Arrange(new Rect(0, 0, 0, height));
            }
            return finalSize;
        }

        double baseWidth = Math.Floor(availableWidthForColumns / count);
        int extra = (int)Math.Floor(availableWidthForColumns - baseWidth * count);

        var allocatedWidths = new double[count];
        for (int i = 0; i < count; i++)
        {
            allocatedWidths[i] = baseWidth;
        }

        if (extra > 0)
        {
            var indexedChildren = children
                .Select((child, index) => new { Index = index, DesiredWidth = child.DesiredSize.Width })
                .OrderByDescending(x => x.DesiredWidth)
                .ThenBy(x => x.Index)
                .Take(extra);

            foreach (var item in indexedChildren)
            {
                allocatedWidths[item.Index] += 1;
            }
        }

        double currentX = 0;
        for (int i = 0; i < count; i++)
        {
            double colWidth = allocatedWidths[i];
            children[i].Arrange(new Rect(currentX, 0, colWidth, height));
            currentX += colWidth + Gap;
        }

        return finalSize;
    }
}