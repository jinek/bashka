using System.Diagnostics.CodeAnalysis;
using Bashka.Plugins.Core.Abstract.Data;
using Bashka.Plugins.Core.Scoping;
using Bashka.Plugins.Core.Worker.Schedulers;
using JetBrains.Annotations;
using PropertyChanged;

namespace Bashka.Plugins.Core.Abstract.Surrounding;

public abstract class PathCatalog : BaseObject, ICatalog
{
    private Task _currentPathLoadingTask = Task.CompletedTask;

    private async Task ChangePathAndPreloadAsync(string newPath)
    {
        await EnsureCurrentPathLoaded();
        _currentPath  = newPath;
        RaiseCurrentPathChanged();
        await EnsureCurrentPathLoaded();
    }

    private async Task EnsureCurrentPathLoaded()
    {
        await _currentPathLoadingTask;
    }

    public string? CurrentPath => _currentPath;

    [DependsOn(nameof(CurrentPath))]
    public abstract string Title { get; }

    private void RaiseCurrentPathChanged()
    {
        OnPropertyChanged(nameof(CurrentPath));
        StartLoadingChildren();
    }

    private void StartLoadingChildren()
    {
        _children = null;
        
        _currentPathLoadingTask = Worker.StartNewWorkerTask(async (cancellationToken, progress, uiTaskFactory) =>
        {
            _children = (await GetChildrenInternal()).ToArray() /*we have to enumerate in this thread!*/;
            // Thread.Sleep(2000);
            
            // ReSharper disable once MethodSupportsCancellation
            await uiTaskFactory.StartNew(() => { OnPropertyChanged(nameof(Children)); });
        }, $"Loading children at {GetType()}");
    }

    private IEnumerable<Entry>? _children;
    private string? _currentPath;

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

    public event Action? OnInitialized;

    public IEnumerable<Entry> Children => _children ?? Array.Empty<Entry>();//todo: create empty static array

    protected abstract Task<IEnumerable<Entry>> GetChildrenInternal();

    public virtual async Task NavigateAsync(string path)
    {
        string newPath = string.IsNullOrEmpty(CurrentPath) ? path : PathJoin(CurrentPath, path); //todo: may be just pathJoin?
        await ChangePathAndPreloadAsync(newPath);
    }
    
    public static string PathJoin(params string[] pathSegments)
    {
        return Path.Combine(pathSegments).Replace(@"\",@"/");
    }

    public static string[] PathSegments(string path2, int number = int.MaxValue)
    {
        string? path = path2;
        var result = new List<string>();
        while (path != string.Empty)
        {
            string fileName = Path.GetFileName(path);
            result.Add(fileName != string.Empty ? fileName : path);
            path = Path.GetDirectoryName(path);
            if (path == null) break;
            if (--number != 0) continue;
            result.Add(path);
            break;
        }

        return result.AsEnumerable().Reverse().ToArray();
    }

    public virtual bool CanNavigateBack => CurrentPath != null && PathSegments(CurrentPath).Length > 1;

    public virtual async Task NavigateBack()
    {
        string[] pathSegments = PathSegments(CurrentPath!);
        if (pathSegments.Length == 0)
            throw new NavigationException();
        string directoryName = PathJoin(pathSegments[..^1]);
        await ChangePathAndPreloadAsync(directoryName);
    }
}
