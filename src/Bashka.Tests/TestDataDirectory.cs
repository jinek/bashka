using Bashka.Helpers;
using Bashka.Plugins.Core.Configuration;

namespace Bashka.Tests;

/// <summary>
///     Keeps tests away from the real Bashka data of the user running them
/// </summary>
[SetUpFixture]
public class TestDataDirectory
{
    private readonly string _directory = Path.Combine(Path.GetTempPath(), $"Bashka.Tests.{Guid.NewGuid():N}");

    [OneTimeSetUp]
    public void SetUp()
    {
        Directory.CreateDirectory(_directory);
        Environment.SetEnvironmentVariable(BashkaHelpers.DataDirectoryVariable, _directory);
        Assert.That(BashkaHelpers.DataDirectory, Is.EqualTo(_directory),
            "data directory was resolved before tests redirected it");
    }

    [OneTimeTearDown]
    public void TearDown()
    {
        if (Directory.Exists(_directory)) Directory.Delete(_directory, true);
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