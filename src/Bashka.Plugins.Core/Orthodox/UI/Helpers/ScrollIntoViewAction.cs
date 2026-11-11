using Avalonia;
using Avalonia.Controls;
using Avalonia.Xaml.Interactivity;
using PropertyChanged;

namespace Bashka.Plugins.Core.Orthodox.UI.Helpers;

[DoNotNotify]
public class ScrollIntoViewAction : AvaloniaObject, IAction
{
    public object Execute(object? sender, object? parameter)
    {
        var dataGrid = (DataGrid)sender!;
        object? item = ((SelectionChangedEventArgs)parameter!).AddedItems?.Cast<object>().FirstOrDefault();
        if (item == null)
            return false;
        
        dataGrid.ScrollIntoView(item, dataGrid.Columns[0]);
        
        return true;
    }
}