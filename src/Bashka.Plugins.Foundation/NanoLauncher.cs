using Avalonia;
using Bashka.Plugins.Foundation.ShellInteraction;
using Consolonia;
using Unix.Terminal;

namespace Bashka.Plugins.Foundation;

public static class NanoLauncher
{
    public static async Task OpenAsync(IPtyExecutor catalog, string workingDir, string path)
    {
        var cts = new CancellationTokenSource();

        await ((ConsoloniaLifetime)Application.Current!.ApplicationLifetime!).DisconnectFromConsoleAsync(
            cts.Token);

        await catalog.ExecuteAsResidentAsync(workingDir, "nano", path, CancellationToken.None)
            .ConfigureAwait(false);
        //todo: following initialization is hack, it works only on curses and copypaste
        Curses.initscr();
        Curses.noecho();
        Curses.cbreak();
        Curses.doupdate();
        Curses.raw();
        Curses.Window.Standard.keypad(true);
        Curses.mousemask(Curses.Event.AllEvents | Curses.Event.ReportMousePosition, out Curses.Event _);
        Console.CursorVisible = false;

        // todo: low: not sure need additional thread
        ThreadPool.QueueUserWorkItem(_ =>
        {
            cts.Cancel(true);
        });
    }
}
