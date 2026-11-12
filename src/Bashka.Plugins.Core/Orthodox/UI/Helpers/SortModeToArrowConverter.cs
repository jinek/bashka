using System.Globalization;
using Avalonia.Data.Converters;
using Bashka.Plugins.Core.Abstract.Data;
using SortKey = Bashka.Plugins.Core.Abstract.Data.SortKey;

namespace Bashka.Plugins.Core.Orthodox.UI.Helpers;

/// <summary>
///     Parameter must go as "field:Up"
/// </summary>
public class SortModeToArrowVisibilityConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        string[] keyAndArrow = ((string)parameter!).Split(':');
        var sortKey = Enum.Parse<SortKey>(keyAndArrow[0]);

        string sortDirectionAsString = value is SortMode sortMode && sortMode.Key == sortKey
            ? !sortMode.Descending ? "Up" : "Down"
            : string.Empty;
        return string.Equals(sortDirectionAsString, keyAndArrow[1], StringComparison.OrdinalIgnoreCase);
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}