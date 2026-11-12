namespace Bashka.Helpers;

/// <summary>
///     Exception which crashes the application
/// </summary>
public class CrashAppException(Exception innerException)
    : Exception("We apologize for the inconvenience. A critical error occurred.", innerException);