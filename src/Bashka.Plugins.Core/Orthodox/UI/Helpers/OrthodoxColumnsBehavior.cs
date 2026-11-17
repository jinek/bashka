using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Media;
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

            AddColumn("Name", nameof(Entry.Name), new DataGridTemplateColumn
            {
                SortMemberPath = nameof(Entry.Name),
                CellTemplate = new FuncDataTemplate<Entry>((_, _) =>
                {
                    var name = new TextBlock { TextTrimming = TextTrimming.CharacterEllipsis };
                    name.Bind(TextBlock.TextProperty,
                        new Binding(nameof(Entry.DisplayName)) { Mode = BindingMode.OneWay });

                    var extension = new TextBlock
                    {
                        TextAlignment = TextAlignment.Right,
                        Margin = new Thickness(1, 0, 0, 0)
                    };
                    extension.Bind(TextBlock.TextProperty,
                        new Binding(nameof(Entry.Extension)) { Mode = BindingMode.OneWay });
                    Grid.SetColumn(extension, 1);

                    return new Grid
                    {
                        Margin = new Thickness(0, 0, 1, 0),
                        ColumnDefinitions = new ColumnDefinitions("*,Auto"),
                        Children = { name, extension }
                    };
                })
            }, null, dockPanel);
        }

        AddColumn("Size", nameof(Entry.Size), new DataGridTextColumn
        {
            Binding = new Binding(nameof(Entry.SelfHack))
            {
                Mode = BindingMode.OneWay,
                Converter = new EntryToFileSizeConverter()
            }
        }, 7);

        AddColumn("Access", nameof(Entry.TimeStamp), new DataGridTextColumn
        {
            Binding = new Binding(nameof(Entry.TimeStamp))
            {
                Converter = new AccessTimeConverter(),
                Mode = BindingMode.OneWay
            }
        }, 10);
    }

    private void AddColumn(string columnTitle, string fieldName, DataGridColumn column, int? size = null,
        Control? additional = null)
    {
        if (size != null)
        {
            column.Width = new DataGridLength((double)size, DataGridLengthUnitType.Pixel);
            column.MaxWidth = (double)size;
            column.MinWidth = (double)size;
        }
        else
        {
            column.Width = new DataGridLength(1, DataGridLengthUnitType.Star);
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

        column.Header = new DockPanel
        {
            LastChildFill = true,
            Children =
            {
                upIcon,
                downIcon,
                mainContent
            }
        };

        AssociatedObject!.Columns.Add(column);
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