using System.Diagnostics;
using System.Runtime.ExceptionServices;
using Bashka.Helpers;

namespace Bashka.Launcher;

internal static partial class Program
{
    public const int ShutdownTimeoutSeconds = 60;

    private static readonly Thread MainThread = Thread.CurrentThread;

    static Program()
    {
        ExceptionHandling.SetUnhandledExceptionHandler(exception =>
        {
            if (Thread.CurrentThread == MainThread)
                return false;

            if (exception is CrashAppException)
            {
                Debug.WriteLine("Application is going to crash");
                if (Debugger.IsAttached)
                    Debugger.Break();
                return false;
            }

            _ = App.ShowApplicationError(exception);
#if HANDLE_CRASH
            // preventing process from crash in RELEASE
            return true;
#else
            return false;
#endif
        });

        TaskScheduler.UnobservedTaskException += (_2, args) =>
        {
            if (args.Exception.InnerException is CrashAppException)
                ThreadPool.QueueUserWorkItem(_ => throw new CrashAppException(args.Exception));

            args.SetObserved();

            if (args.Exception.InnerException!.Data.Contains(App.HandledExceptionHackMark))
                return;

            if (Debugger.IsAttached)
            {
                Debug.WriteLine("An unobserved exception was thrown.");
                Debugger.Break();
            }

            _ = App.ShowApplicationError(args.Exception);
        };
    }
}