using System.ComponentModel;
using System.Runtime.CompilerServices;
using Bashka.Plugins.Core.Abstract;
using Bashka.Plugins.Core.Abstract.Data;
using PropertyChanged;

namespace Bashka.Plugins.Core.FunctionKeys;

public class FunctionBarBinding : INotifyPropertyChanged
{
    private CommandExecutionContext? _context;

    [AlsoNotifyFor(nameof(CanExecute))]
    [DoNotCheckEquality]
    internal CommandExecutionContext? Context
    {
        get => _context;
        set => _context = value;
    }

    public ICommand? Command { get; init; }
    public required string Name { get; init; }
    public bool IsDelete { get; init; }
    public bool CanExecute => Context != null && Command?.CanExecute(Context) == true;
    
    public event PropertyChangedEventHandler? PropertyChanged;

    protected internal virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }
}