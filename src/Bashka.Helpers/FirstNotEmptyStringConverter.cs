using System.Globalization;
using Avalonia;
using Avalonia.Data.Converters;

namespace Bashka.Helpers;

public class FirstNotEmptyStringConverter : IMultiValueConverter
{
    public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        return values.Count == 0 ? null : values.FirstOrDefault(o => o is not UnsetValueType && !string.IsNullOrEmpty((string?)o));
    }
}