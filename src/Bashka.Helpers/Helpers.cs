using System.Runtime.CompilerServices;
using Avalonia.Controls;
using Avalonia.LogicalTree;
using Avalonia.Threading;

namespace Bashka.Helpers
{
    public static class Helpers
    {
        public static T GetElement<T>(this ILogical visual, [CallerMemberName] string? elementName = null)
            where T : class, ILogical
        {
            return visual.Get<T>(elementName!);
        }

        public static void HandleErrors(this Task task)
        {
            task.ContinueWith(
                task1 =>
                {
                    if (task1.Exception!.InnerException is CrashAppException)
                    {
                        ThreadPool.QueueUserWorkItem(_ => throw new CrashAppException(task1.Exception));
                        return;
                    }

                    //surfacing the exception on the dispatcher right away instead of waiting for finalizer of the unobserved task
                    Dispatcher.UIThread.Post(() =>
                        throw new InvalidOperationException("Exception happened in unawaited Task", task1.Exception));
                }, TaskContinuationOptions.OnlyOnFaulted);
        }
    }
}