using System.Globalization;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Input;

namespace Bashka.Plugins.Core.Orthodox.UI.Helpers;

public class KeyEventToModelConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var keyEventArgs = (KeyEventArgs)value!;
        return keyEventArgs.Handled ? BindingOperations.DoNothing : new KeyDownModel(keyEventArgs);
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}