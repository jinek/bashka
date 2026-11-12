using System.Globalization;
using Avalonia;
using Avalonia.Data.Converters;
using Bashka.Plugins.Core.Abstract.Data;
using Bashka.Plugins.Core.Abstract.Surrounding;

namespace Bashka.Plugins.Core.Orthodox.UI.Helpers;

public class EntryToFileSizeConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is UnsetValueType)
            return value;
        
        var entry = (Entry)value!;
        if (entry.IsGone)
            return "N/A";
        if (entry is GoBackEntry) return "Up";
        return entry.IsDirectory ? "Folder" : FormatSize(entry.Size);
    }

    // code from https://www.c-sharpcorner.com/article/csharp-convert-bytes-to-kb-mb-gb/
    // Load all suffixes in an array  
    private static readonly string[] Suffixes =
        ["Bytes", "KB", "MB", "GB", "TB", "PB"];

    private static string FormatSize(Int64 bytes)
    {
        int counter = 0;
        decimal number = bytes;
        while (Math.Round(number / 1024) >= 1)
        {
            number = number / 1024;
            counter++;
        }

        return $"{number:n1}{Suffixes[counter]}";
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}