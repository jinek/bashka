using Bashka.Plugins.Core.Configuration;

namespace Bashka.Tests;

[SetUpFixture]
public class TestDataDirectory
{
    [OneTimeSetUp]
    public void SetUp()
    {
#if LOCAL_DATA
        if (Directory.Exists(BashkaHelpers.DataDirectory)) Directory.Delete(BashkaHelpers.DataDirectory, true);
#endif
    }

    /// <summary>
    ///     Settings file names are fixed, so tests touching them start from empty settings
    /// </summary>
    internal static void DeleteSettings()
    {
        if (Directory.Exists(Settings.UserDirectory)) Directory.Delete(Settings.UserDirectory, true);
        File.Delete(Settings.StateFilePath);
    }
}