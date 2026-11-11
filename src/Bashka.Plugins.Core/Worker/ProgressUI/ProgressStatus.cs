namespace Bashka.Plugins.Core.Worker.ProgressUI;

public struct ProgressStatus
{
    public ProgressStatus(byte progress, string status)
    {
        Progress = progress;
        Status = status;
    }

    public byte Progress { get; }
    public string Status { get; }
}