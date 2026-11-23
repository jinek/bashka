using Autofac;
using Avalonia;
using Bashka.Helpers;
using Consolonia;
using Consolonia.Fonts;

namespace Bashka.Launcher;

public static class DevelopmentHost
{
    internal static Module? PluginModule { get; private set; }

    public static void Run(string[] args, Module? pluginModule = null)
    {
        PluginModule = pluginModule;

        try
        {
            AppBuilder.Configure<App>()
                .LogToException()
                .UseSkia()
                .UseConsolonia()
                .UseAutoDetectedConsole()
                .WithConsoleFonts()
                .ThrowOnErrors()
                .StartWithConsoleLifetime(args);
        }
        catch (Exception exception)
        {
            // the lifetime has not started, so nothing leaves the alternate screen and the crash report is lost there
            (ConsoloniaLifetime.Console as IDisposable)?.Dispose();

            throw new CrashAppException(exception);
        }
    }
}