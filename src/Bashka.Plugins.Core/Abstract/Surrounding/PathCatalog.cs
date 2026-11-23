using System.Diagnostics.CodeAnalysis;
using Bashka.Helpers;
using Bashka.Plugins.Core.Abstract.Data;
using Bashka.Plugins.Core.Scoping;
using Bashka.Plugins.Core.Worker.Schedulers;
using JetBrains.Annotations;
using PropertyChanged;

namespace Bashka.Plugins.Core.Abstract.Surrounding;

public abstract class PathCatalog : BaseObject, ICatalog
{
    private IEnumerable<Entry>? _children;
    private Task _currentPathLoadingTask = Task.CompletedTask;

    [AutoWire]
    [field: MaybeNull]
    // ReSharper disable once MemberCanBePrivate.Global Required by Autofac
    internal WorkerTaskFactory Worker
    {
        get;
        [UsedImplicitly]
        set
        {
            field = value;
            OnInitialized?.Invoke();
        }
    }

    public string? CurrentPath { get; private set; }

    [DependsOn(nameof(CurrentPath))] public abstract string Title { get; }

    public IEnumerable<Entry> Children => _children ?? [];

    public virtual async Task NavigateAsync(string path)
    {
        await ChangePathAndPreloadAsync(BashkaHelpers.PathJoin(CurrentPath ?? string.Empty, path));
    }

    public virtual bool CanNavigateBack => CurrentPath != null && BashkaHelpers.PathSegments(CurrentPath).Length > 1;

    public virtual async Task NavigateBack()
    {
        string[] pathSegments = BashkaHelpers.PathSegments(CurrentPath!);
        if (pathSegments.Length == 0)
            throw new NavigationException();
        string directoryName = BashkaHelpers.PathJoin(pathSegments[..^1]);
        await ChangePathAndPreloadAsync(directoryName);
    }

    private async Task ChangePathAndPreloadAsync(string newPath)
    {
        await EnsureCurrentPathLoaded();
        CurrentPath = newPath;
        RaiseCurrentPathChanged();
        await EnsureCurrentPathLoaded();
    }

    private async Task EnsureCurrentPathLoaded()
    {
        await _currentPathLoadingTask;
    }

    private void RaiseCurrentPathChanged()
    {
        OnPropertyChanged(nameof(CurrentPath));
        StartLoadingChildren();
    }

    private void StartLoadingChildren()
    {
        //todo: F35896B1-7D8B-4E44-A6C4-367F8801EFE2 exceptions crossing dispatcher (ui tasks) somehow not hitting the dispatcher.Exception events 
        _children = null;

        _currentPathLoadingTask = Worker.StartNewWorkerTask(async (_, _, uiTaskFactory) =>
            {
                _children = (await GetChildrenInternal()).ToArray() /*we have to enumerate in this thread!*/;
                // Thread.Sleep(2000);

                // ReSharper disable once MethodSupportsCancellation
                await uiTaskFactory.StartNew(() => { OnPropertyChanged(nameof(Children)); });
            }, $"Loading children at {GetType()}");
    }

    public event Action? OnInitialized;

    protected abstract Task<IEnumerable<Entry>> GetChildrenInternal();
}