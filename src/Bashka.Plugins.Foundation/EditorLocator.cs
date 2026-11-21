namespace Bashka.Plugins.Foundation;

/// <summary>
///     Vibe-coded
/// </summary>
public static class EditorLocator
{
    public static string? FindEditor(string editor)
    {
        return FindEditor(editor, OperatingSystem.IsWindows(),
            (Environment.GetEnvironmentVariable("PATH") ?? string.Empty)
            .Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries));
    }

    public static string? FindEditor(string editor, bool windows, IEnumerable<string> searchDirectories)
    {
        editor = editor.Trim();
        if (Path.IsPathFullyQualified(editor))
            return IsExecutable(editor, windows) ? editor : null;

        string[] directories = searchDirectories.Select(directory => directory.Trim('"'))
            .Where(Path.IsPathFullyQualified).ToArray();
        bool auto = editor.Length == 0 ||
                    editor.Equals(TextEditorSettings.AutoEditor, StringComparison.OrdinalIgnoreCase);
        string[] editors = auto
            ? windows
                ? ["edit.net", "msedit", "edit", "micro", "nano"]
                : ["edit.net", "micro", "msedit", "nano"]
            : [editor];
        string[] extensions = windows ? auto ? [".exe", ".com"] : [".exe", ".com", string.Empty] : [string.Empty];
        return (from name in editors
                from directory in directories
                from extension in extensions
                select auto ? Path.Combine(directory, name + extension) : FindIgnoringCase(directory, name + extension))
            .FirstOrDefault(executable => executable != null && IsExecutable(executable, windows));
    }

    private static string? FindIgnoringCase(string directory, string fileName)
    {
        try
        {
            string exact = Path.Combine(directory, fileName);
            if (File.Exists(exact) || !Directory.Exists(directory)) return exact;
            return Directory.EnumerateFiles(directory)
                .FirstOrDefault(file => Path.GetFileName(file).Equals(fileName, StringComparison.OrdinalIgnoreCase));
        }
        catch (UnauthorizedAccessException)
        {
            return null;
        }
        catch (DirectoryNotFoundException)
        {
            return null;
        }
    }

    private static bool IsExecutable(string path, bool windows)
    {
        try
        {
            if (!File.Exists(path)) return false;
            return windows || OperatingSystem.IsWindows() ||
                   (File.GetUnixFileMode(path) &
                    (UnixFileMode.UserExecute | UnixFileMode.GroupExecute | UnixFileMode.OtherExecute)) != 0;
        }
        catch (UnauthorizedAccessException)
        {
            return false;
        }
    }
}