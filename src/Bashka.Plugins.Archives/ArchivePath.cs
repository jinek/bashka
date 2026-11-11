namespace Bashka.Plugins.Archives;

/// <summary>
/// Paths inside of archives are always slash separated and never start or end with a slash.
/// </summary>
public static class ArchivePath
{
    public const char Separator = '/';

    public static string Normalize(string path)
    {
        string result = path.Replace('\\', Separator).Trim(Separator);

        //tar stores paths like ./sub/file
        while (result.StartsWith($".{Separator}", StringComparison.Ordinal))
            result = result[2..].Trim(Separator);

        return result == "." ? string.Empty : result;
    }
}
