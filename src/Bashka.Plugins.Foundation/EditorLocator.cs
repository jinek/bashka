namespace Bashka.Plugins.Foundation;

/// <summary>
///     vibe-coded, will be replaced by settings
/// </summary>
public static class EditorLocator
{
    public static string? FindEditor()
    {
        return FindEditor(OperatingSystem.IsWindows(),
            (Environment.GetEnvironmentVariable("PATH") ?? string.Empty)
            .Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries));
    }

    public static string? FindEditor(bool windows, IEnumerable<string> searchDirectories)
    {
        string[] directories = searchDirectories.Select(directory => directory.Trim('"'))
            .Where(Path.IsPathFullyQualified).ToArray();
        string[] editors = windows
            ? ["edit.net", "msedit", "edit", "micro", "nano"]
            : ["edit.net", "micro", "msedit", "nano"];
        string[] extensions = windows ? [".exe", ".com"] : [string.Empty];
        foreach (string editor in editors)
        foreach (string directory in directories)
        foreach (string extension in extensions)
        {
            string executable = Path.Combine(directory, editor + extension);
            if (!File.Exists(executable)) continue;
            if (!windows && !OperatingSystem.IsWindows() &&
                (File.GetUnixFileMode(executable) &
                 (UnixFileMode.UserExecute | UnixFileMode.GroupExecute | UnixFileMode.OtherExecute)) == 0)
                continue;
            return executable;
        }

        return null;
    }
}