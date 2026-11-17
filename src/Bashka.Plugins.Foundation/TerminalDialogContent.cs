using Bashka.Plugins.Core.Dialog.Abstract;

namespace Bashka.Plugins.Foundation;

public sealed class TerminalDialogContent(string workingDirectory, string applicationName, string argument)
    : DialogContent(applicationName, "small_margin")
{
    public string WorkingDirectory { get; } = workingDirectory;
    public string ApplicationName { get; } = applicationName;
    public IReadOnlyList<string> Arguments { get; } = string.IsNullOrEmpty(argument) ? [] : [argument];

    public void Close()
    {
        RaiseClose();
    }
}