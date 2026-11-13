namespace Bashka.Helpers;

public static class BashkaPaths
{
    public static readonly string DataDirectory = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Bashka");
}
