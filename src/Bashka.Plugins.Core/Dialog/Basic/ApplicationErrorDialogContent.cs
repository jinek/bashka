using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input.Platform;
using Avalonia.Threading;
using Bashka.Plugins.Core.Dialog.Abstract;

namespace Bashka.Plugins.Core.Dialog.Basic;

public class ApplicationErrorDialogContent : DialogContent
{
    private const string NewIssueUrl = "https://github.com/jinek/bashka/issues/new";

    private readonly Exception _exception;
    private readonly DispatcherTimer _timer = new() { Interval = TimeSpan.FromSeconds(1) };

    public ApplicationErrorDialogContent(Exception exception, int shutdownTimeoutSeconds)
        : base("Unexpected Error", "centered", "min_size", "normal_background")
    {
        _exception = exception;
        ExceptionText = exception.ToString();
        SecondsRemaining = shutdownTimeoutSeconds;
        _timer.Tick += (_, _) =>
        {
            if (SecondsRemaining > 0)
                SecondsRemaining--;
            else
                _timer.Stop();
        };
        _timer.Start();
    }

    public string ExceptionText { get; }
    public static string ReportText => $"Please report this issue on GitHub: {NewIssueUrl}";
    public int SecondsRemaining { get; private set; }

    public async void Report()
    {
        TopLevel? topLevel = (Application.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)
            ?.MainWindow;

        if (topLevel?.Clipboard != null)
            await topLevel.Clipboard.SetTextAsync(ExceptionText);

        if (topLevel?.Launcher != null)
            await topLevel.Launcher.LaunchUriAsync(new Uri(NewIssueUrl));
    }

    public void TerminateAndPrint()
    {
        _timer.Stop();
        Console.WriteLine(ExceptionText);

        if (Application.Current?.ApplicationLifetime is IControlledApplicationLifetime controlledLifetime)
            controlledLifetime.Shutdown(1);
        else
            Environment.FailFast(ExceptionText, _exception);
    }

    public void Ignore()
    {
        _timer.Stop();
        RaiseClose();
    }
}