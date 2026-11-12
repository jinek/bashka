using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Autofac;
using JetBrains.Annotations;

namespace Bashka.Plugins.Core.Scoping;

public class BaseObject : TypeKeyedObject, INotifyPropertyChanged
{
    [AutoWire]
    [field: MaybeNull]
    public ILifetimeScope LifetimeScope
    {
        get;
        [UsedImplicitly]
        internal set
        {
            field = value;
            OnScopeInitialized();
        }
    } = null!;

    public event PropertyChangedEventHandler? PropertyChanged;

    [NotifyPropertyChangedInvocator]
    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    [NotifyPropertyChangedInvocator]
    protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }

    protected void NotifyDependentProperties(string sourcePropertyName, params string[] dependentPropertyNames)
    {
        PropertyChanged += (_, args) =>
        {
            if (args.PropertyName != sourcePropertyName)
                return;

            foreach (string dependentPropertyName in dependentPropertyNames)
                OnPropertyChanged(dependentPropertyName);
        };
    }

    protected virtual void OnScopeInitialized()
    {
    }
}