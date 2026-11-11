using System.Globalization;
using Avalonia.Data.Converters;

namespace Bashka.Plugins.Core.Orthodox.UI.Helpers;

public class EqualsConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value == null)
            return parameter == null;

        return value.Equals(parameter);
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}