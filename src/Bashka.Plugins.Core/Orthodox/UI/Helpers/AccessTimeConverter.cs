using System.Globalization;
using Avalonia;
using Avalonia.Data.Converters;

namespace Bashka.Plugins.Core.Orthodox.UI.Helpers;

public class AccessTimeConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is UnsetValueType) return value;

        var date = (DateTime)value!;
        return date == DateTime.MinValue ? string.Empty : date.ToString("d", culture);
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}