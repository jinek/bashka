namespace Bashka.Plugins.Core.Worker.Schedulers;

/// <summary>
///     Carries an exception of a UI task to the dispatcher. The original exception can't be rethrown there itself:
///     Avalonia does not catch an exception it has already seen, and the awaiter of the task rethrows it on the dispatcher again.
/// </summary>
public sealed class UITaskFaultedException(Exception innerException)
    : Exception("Exception happened in a task executed on UI", innerException);
