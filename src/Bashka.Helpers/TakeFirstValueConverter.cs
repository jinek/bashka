using System.Globalization;
using Avalonia.Data.Converters;

namespace Bashka.Helpers;

public class TakeFirstValueConverter : IMultiValueConverter
{
    public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        //todo: why is called twice after only one multiConversion
        return FirstValueConverter.Convert(values[0], null, null, culture);
    }

    public IValueConverter FirstValueConverter { get; set; }
}