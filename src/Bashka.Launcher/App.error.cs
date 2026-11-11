using Autofac;
using Avalonia.Threading;
using Bashka.Helpers;
using Bashka.Plugins.Core.Abstract.Surrounding;
using Bashka.Plugins.Core.Dialog.Abstract;
using Bashka.Plugins.Core.Dialog.Basic;

namespace Bashka.Launcher;

public partial class App
{
    // todo: everything regarding globall error handling was copypasted by claude from Edit.NET, I did not check it enough
    /// <summary>
    ///     Avalonia rethrows an exception to unobserved task, which we also handle. Thus marking this exception to know it
    ///     was handled already.
    ///     Why not just to show it from UnobservedTask? Because we don't know when that will happen (finalizer call)
    /// </summary>
    internal static readonly object HandledExceptionHackMark = new();

    private static void HandleDispatcherExceptions()
    {
        Dispatcher.UIThread.UnhandledExceptionFilter += async (_, args) =>
        {
            switch (args.Exception)
            {
                case UserCancelException:
                    args.Exception.Data[HandledExceptionHackMark] = null;
                    args.RequestCatch = true;
                    break;
                case UserErrorException userErrorException:
                    args.Exception.Data[HandledExceptionHackMark] = null;
                    args.RequestCatch = true;
                    await ShowUserError(userErrorException);
                    break;
                case CrashAppException:
                    args.RequestCatch = false;
                    //let it crash
                    break;
                default:
#if HANDLE_CRASH
                    args.RequestCatch = true;
                    args.Exception.Data[HandledExceptionHackMark] = null;
                    await ShowApplicationError(args.Exception);
#else
                    args.RequestCatch = false;
#endif
                    break;
            }
        };

        Dispatcher.UIThread.UnhandledException += (_, args) => { args.Handled = true; };
    }

    private static async Task ShowUserError(UserErrorException userErrorException)
    {
        var dialogProvider = MainWindow.Scope.Resolve<IDialogProvider>();
        await dialogProvider.ShowDialogAsync(new ErrorDialogContent(userErrorException.Message));
    }

    /// <summary>
    ///     Try to show a dialog with the error.
    ///     If the dialog is not closed within <see cref="Program.ShutdownTimeoutSeconds" /> seconds or if it throws an
    ///     exception the application will do a regular crash
    /// </summary>
    /// <param name="exception">Exception to report</param>
    internal static async Task ShowApplicationError(Exception exception)
    {
        var tsc = new CancellationTokenSource();
        var wrappedException = new CrashAppException(exception);

        _ = Task.Delay(TimeSpan.FromSeconds(Program.ShutdownTimeoutSeconds), tsc.Token)
            .ContinueWith(_ =>
                {
                    // crashing the application because dialog was not closed in time
                    CrashViaThreadPool();
                }, CancellationToken.None,
                TaskContinuationOptions.OnlyOnRanToCompletion, TaskScheduler.Default);
        try
        {
            await Dispatcher.UIThread.InvokeAsync(async () =>
            {
                var dialogProvider = MainWindow.Scope.ResolveOptional<IDialogProvider>();
                if (dialogProvider == null)
                {
                    // crashing the application because dialog can not be shown yet
                    CrashViaThreadPool();
                    return;
                }

                await dialogProvider.ShowDialogAsync(new ErrorDialogContent(exception.ToString()));
            });
        }
        catch (Exception secondaryException)
        {
            // crashing the application because dialog can not be shown
            wrappedException = new CrashAppException(new AggregateException(exception, secondaryException));
            CrashViaThreadPool();
            throw;
        }

        await tsc.CancelAsync();

        return;

        void CrashViaThreadPool()
        {
            // ReSharper disable once AccessToModifiedClosure  It's intentionally!! Yes, we want to modify captured variable, why...
            ThreadPool.QueueUserWorkItem(_ => throw wrappedException);
        }
    }
}
