using System.Runtime.CompilerServices;
using Avalonia.Controls;
using Avalonia.LogicalTree;

namespace Bashka.Helpers;

public static class BashkaHelpers
{
    public const string DataDirectoryVariable = "BASHKA_DATA_DIR";

    /// <summary>
    ///     Can be redirected with <see cref="DataDirectoryVariable" /> environment variable
    /// </summary>
    public static readonly string DataDirectory =
        Environment.GetEnvironmentVariable(DataDirectoryVariable) is { Length: > 0 } dataDirectory
            ? Path.GetFullPath(dataDirectory)
            : Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Bashka");

    public static T GetElement<T>(this ILogical visual, [CallerMemberName] string? elementName = null)
        where T : class, ILogical
    {
        return visual.Get<T>(elementName!);
    }
}