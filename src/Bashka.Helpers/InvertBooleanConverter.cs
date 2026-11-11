using System.Globalization;
using Avalonia.Data.Converters;

namespace Bashka.Helpers;

public class InvertBooleanConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return !(bool)(value ?? false); //todo: may be throw exception instead of default false
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return !(bool)(value ?? false);
    }
}