using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Xaml.Interactivity;
using Bashka.Plugins.Core.Abstract.Data;
using Consolonia.Controls;
using PropertyChanged;

namespace Bashka.Plugins.Core.Orthodox.UI.Helpers;

[DoNotNotify]
public class OrthodoxColumnsBehavior : Behavior<VerticalScrollDataGrid>
{
    protected override void OnAttached()
    {
        base.OnAttached();

        if (AssociatedObject == null)
            return;

        {
            (SymbolsControl upIcon, SymbolsControl downIcon) = GetDirectionIcons("Extension");

            var textBlockRight = new TextBlock { Text = ")" };
            textBlockRight.SetValue(DockPanel.DockProperty, Dock.Right);
            var dockPanel = new DockPanel
            {
                Margin = new Thickness(1, 0, 0, 0),
                Children =
                {
                    new TextBlock { Text = "(Ext" },
                    textBlockRight,
                    upIcon,
                    downIcon
                }
            };
            dockPanel.SetValue(DockPanel.DockProperty, Dock.Right);
            dockPanel.Bind(Visual.IsVisibleProperty, new MultiBinding
            {
                Converter = new BooleanOrConverter(),
                Bindings =
                {
                    CreateBinding("Extension:Up"),
                    CreateBinding("Extension:Down")
                }
            });

            AddColumn("Name", nameof(Entry.Name), new Binding(nameof(Entry.Name)) { Mode = BindingMode.OneWay }, null,
                dockPanel);
        }

        AddColumn("Size", nameof(Entry.Size), new Binding
        {
            Path = "SelfHack",
            Mode = BindingMode.OneWay,
            Converter = new EntryToFileSizeConverter()
        }, 7);

        AddColumn("Access", nameof(Entry.TimeStamp), new Binding(nameof(Entry.TimeStamp))
        {
            StringFormat = "{0:d}",
            Mode = BindingMode.OneWay
        }, 10);
    }

    private void AddColumn(string columnTitle, string fieldName, Binding columnBinding, int? size = null,
        Control? additional = null)
    {
        var dataGridTextColumn = new DataGridTextColumn
        {
            Binding = columnBinding
        };

        if (size != null)
        {
            dataGridTextColumn.Width = new DataGridLength((double)size, DataGridLengthUnitType.Pixel);
            dataGridTextColumn.MaxWidth = (double)size;
            dataGridTextColumn.MinWidth = (double)size;
        }
        else
        {
            dataGridTextColumn.Width = new DataGridLength(1, DataGridLengthUnitType.Star);
        }

        (SymbolsControl upIcon, SymbolsControl downIcon) = GetDirectionIcons(fieldName);

        Control mainContent = new TextBlock
        {
            Text = columnTitle
        };
        if (additional != null)
            mainContent = new DockPanel
            {
                LastChildFill = true,
                Children =
                {
                    additional,
                    mainContent
                }
            };

        dataGridTextColumn.Header = new DockPanel
        {
            LastChildFill = true,
            Children =
            {
                upIcon,
                downIcon,
                mainContent
            }
        };

        AssociatedObject!.Columns.Add(dataGridTextColumn);
    }

    private static (SymbolsControl upIcon, SymbolsControl downIcon) GetDirectionIcons(string fieldName)
    {
        var upIcon = new SymbolsControl
        {
            Text = "▲",
            Fill = false,
            Margin = new Thickness(1, 0, 0, 0)
        };
        upIcon.Bind(Visual.IsVisibleProperty,
            CreateBinding($"{fieldName}:Up"));
        upIcon.SetValue(DockPanel.DockProperty, Dock.Right);
        var downIcon = new SymbolsControl
        {
            Text = "▼",
            Fill = false,
            Margin = new Thickness(1, 0, 0, 0)
        };
        downIcon.Bind(Visual.IsVisibleProperty,
            CreateBinding($"{fieldName}:Down"));
        downIcon.SetValue(DockPanel.DockProperty, Dock.Right);
        return (upIcon, downIcon);
    }

    private static Binding CreateBinding(string upOrDown)
    {
        return new Binding("DataContext.SortMode")
        {
            RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor)
            {
                AncestorType = typeof(VerticalScrollDataGrid)
            },
            Converter = new SortModeToArrowVisibilityConverter(),
            ConverterParameter = upOrDown,
            Mode = BindingMode.OneWay
        };
    }
}

public class BooleanOrConverter : IMultiValueConverter
{
    public object Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values.Any(o => o is not bool))
            return false;
        return values.Cast<bool>().Aggregate(false, (b, b1) => b || b1);
    }
}