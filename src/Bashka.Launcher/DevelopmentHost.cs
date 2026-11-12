using Autofac;
using Avalonia;
using Consolonia;
using Consolonia.Fonts;

namespace Bashka.Launcher;

public static class DevelopmentHost
{
    internal static Module? PluginModule { get; private set; }

    public static void Run(string[] args, Module? pluginModule = null)
    {
        PluginModule = pluginModule;

        AppBuilder.Configure<App>()
            .LogToException()
            .UseSkia()
            .UseConsolonia()
            .UseAutoDetectedConsole()
            .WithConsoleFonts()
            .ThrowOnErrors()
            .StartWithConsoleLifetime(args);
    }
}