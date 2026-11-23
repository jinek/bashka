using System.Runtime.CompilerServices;
using Avalonia.Controls;
using Avalonia.LogicalTree;

namespace Bashka.Helpers;

public static class BashkaHelpers
{
    private const char DirectorySeparator = '/';
    private static readonly char[] DirectorySeparators = [DirectorySeparator, Path.DirectorySeparatorChar];

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

    public static string PathJoin(params string[] pathSegments)
    {
        var result = string.Empty;
        foreach (string segment in pathSegments)
        {
            if (segment.Length == 0)
                continue;
            if (result.Length == 0 || Path.IsPathRooted(segment))
                result = segment;
            else if (DirectorySeparators.Contains(result[^1]))
                result += segment;
            else
                result = $"{result}{DirectorySeparator}{segment}";
        }

        return result;
    }

    public static string[] PathSegments(string path)
    {
        string root = path.StartsWith(DirectorySeparator)
            ? DirectorySeparator.ToString()
            : Path.GetPathRoot(path) ?? string.Empty;
        string[] names = path[root.Length..].Split(DirectorySeparators, StringSplitOptions.RemoveEmptyEntries);
        return root.Length == 0 ? names : [root, ..names];
    }
}
