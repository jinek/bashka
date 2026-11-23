using System.Globalization;
using Avalonia.Data.Converters;
using Bashka.Plugins.Core.Abstract.Data;

namespace Bashka.Plugins.Core.Orthodox.UI.Helpers;

public class MarkedEntriesSummaryConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not IReadOnlyList<Entry> { Count: > 0 } entries)
            return null;

        Entry[] files = [.. entries.Where(entry => !entry.IsDirectory)];
        int folders = entries.Count - files.Length;
        string size = EntryToFileSizeConverter.FormatSize(files.Where(file => file.Size > 0).Sum(file => file.Size));
        return $"Selected {entries.Count}: {Plural(files.Length, "file")}, {Plural(folders, "folder")}, {size}";

        static string Plural(int count, string noun)
        {
            return count == 1 ? $"1 {noun}" : $"{count} {noun}s";
        }
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}