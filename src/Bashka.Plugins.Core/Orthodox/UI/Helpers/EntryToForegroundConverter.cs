using System.Globalization;
using Avalonia;
using Avalonia.Data.Converters;
using Avalonia.Media;
using Bashka.Plugins.Core.Abstract.Data;
using PropertyChanged;

namespace Bashka.Plugins.Core.Orthodox.UI.Helpers;

[DoNotNotify]
public class EntryToForegroundConverter : AvaloniaObject, IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var entry = (Entry)value!;
        if (entry.IsGone)
            return DeletedBrush;
        if (entry.IsMarked)
            return MarkedBrush;
        if (entry.Category is EntryCategory.Hidden)
            return HiddenBrush;
        if (entry.IsDirectory)
            return DirectoryBrush;
        return entry.Category switch
        {
            EntryCategory.Executable => ExecutableBrush,
            EntryCategory.Important => ImportantBrush,
            _ => ItemBrush
        };
    }

    public static readonly StyledProperty<Brush> ItemBrushProperty =
        AvaloniaProperty.Register<EntryToForegroundConverter, Brush>(
            nameof(ItemBrush));

    public Brush ItemBrush
    {
        get => GetValue(ItemBrushProperty);
        set => SetValue(ItemBrushProperty, value);
    }

    public static readonly StyledProperty<Brush> DirectoryBrushProperty =
        AvaloniaProperty.Register<EntryToForegroundConverter, Brush>(
            nameof(DirectoryBrush));

    public Brush DirectoryBrush
    {
        get => GetValue(DirectoryBrushProperty);
        set => SetValue(DirectoryBrushProperty, value);
    }
    
    public static readonly StyledProperty<Brush> HiddenBrushProperty =
        AvaloniaProperty.Register<EntryToForegroundConverter, Brush>(
            nameof(HiddenBrush));

    public Brush HiddenBrush
    {
        get => GetValue(HiddenBrushProperty);
        set => SetValue(HiddenBrushProperty, value);
    }
    
    public static readonly StyledProperty<Brush> ImportantBrushProperty =
        AvaloniaProperty.Register<EntryToForegroundConverter, Brush>(
            nameof(ImportantBrush));

    public Brush ImportantBrush
    {
        get => GetValue(ImportantBrushProperty);
        set => SetValue(ImportantBrushProperty, value);
    }
    
    public static readonly StyledProperty<Brush> ExecutableBrushProperty =
        AvaloniaProperty.Register<EntryToForegroundConverter, Brush>(
            nameof(ExecutableBrush));

    public Brush ExecutableBrush
    {
        get => GetValue(ExecutableBrushProperty);
        set => SetValue(ExecutableBrushProperty, value);
    }
    
    public static readonly StyledProperty<Brush> MarkedBrushProperty =
        AvaloniaProperty.Register<EntryToForegroundConverter, Brush>(
            nameof(MarkedBrush));

    public Brush MarkedBrush
    {
        get => GetValue(MarkedBrushProperty);
        set => SetValue(MarkedBrushProperty, value);
    }
    
    public static readonly StyledProperty<Brush> DeletedBrushProperty =
        AvaloniaProperty.Register<EntryToForegroundConverter, Brush>(
            nameof(DeletedBrush));

    public Brush DeletedBrush
    {
        get => GetValue(DeletedBrushProperty);
        set => SetValue(DeletedBrushProperty, value);
    }
    
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}