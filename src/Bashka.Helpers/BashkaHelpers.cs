using System.Runtime.CompilerServices;
using Avalonia.Controls;
using Avalonia.LogicalTree;

namespace Bashka.Helpers;

public static class BashkaHelpers
{
    /// <summary>
    ///     Builds made outside CI keep data next to the binaries
    /// </summary>
    public static readonly string DataDirectory =
#if LOCAL_DATA
        Path.Combine(AppContext.BaseDirectory, "BashkaData");
#else
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Bashka");
#endif

    public static bool IsStartedByLauncher =>
        new[] { Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), AppContext.BaseDirectory }
            .Select(Normalize)
            .Contains(Normalize(Environment.CurrentDirectory));

    private static string Normalize(string directory)
    {
        return Path.TrimEndingDirectorySeparator(Path.GetFullPath(directory));
    }

    public static T GetElement<T>(this ILogical visual, [CallerMemberName] string? elementName = null)
        where T : class, ILogical
    {
        return visual.Get<T>(elementName!);
    }
}