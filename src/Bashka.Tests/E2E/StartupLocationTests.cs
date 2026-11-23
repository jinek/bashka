using Autofac;
using Avalonia;
using Bashka.Launcher;
using Bashka.Plugins.Core.Configuration;
using Bashka.Plugins.Core.Orthodox;
using Bashka.Tests.Helpers;

namespace Bashka.Tests.E2E;

/// <summary>
///     Tests run in the directory of the application, as when it is started by a launcher
/// </summary>
[TestFixture]
public class StartupLocationTests : BashkaAppTestBase
{
    private static string Home => Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
    internal static string Sandbox => Path.Combine(TestContext.CurrentContext.TestDirectory, "E2ESandbox");
    internal static OrthodoxCommander Commander => MainWindow.Scope.Resolve<OrthodoxCommander>();

    protected override AppBuilder CreateAppBuilder()
    {
        AppBuilder builder = base.CreateAppBuilder();
        TestDataDirectory.DeleteSettings();
        Directory.CreateDirectory(Settings.UserDirectory);
        File.WriteAllText(Path.Combine(Settings.UserDirectory, "startup.ini"),
            "[OrthodoxCommander]\nStartupLocationLeft = %HOME%\n");
        File.WriteAllText(Settings.StateFilePath, $"[OrthodoxCommander(state)]\nLeftPath = {Sandbox}\nRightPath = {Sandbox}\n");
        return builder;
    }

    [TearDown]
    public void DeleteSettings()
    {
        TestDataDirectory.DeleteSettings();
    }

    [Test]
    public async Task StartupLocationWinsOverSavedOne()
    {
        await UITest.WaitNoText("Processing");
        await UITest.WaitBuffer(_ => Commander.LeftPanel?.CurrentCatalog?.CurrentPath == Home &&
                                     Commander.RightPanel?.CurrentCatalog?.CurrentPath == Sandbox,
            "Left panel is not at the startup location or right panel is not at the saved one");
    }
}

[TestFixture]
public class StartupFromTerminalTests : BashkaAppTestBase
{
    private static string Sandbox => StartupLocationTests.Sandbox;
    private static OrthodoxCommander Commander => StartupLocationTests.Commander;
    private string _previousDirectory = null!;

    protected override AppBuilder CreateAppBuilder()
    {
        AppBuilder builder = base.CreateAppBuilder();
        TestDataDirectory.DeleteSettings();
        Directory.CreateDirectory(Settings.UserDirectory);
        File.WriteAllText(Path.Combine(Settings.UserDirectory, "startup.ini"),
            "[OrthodoxCommander]\nStartupLocationLeft = %HOME%\n");
        File.WriteAllText(Settings.StateFilePath, "[OrthodoxCommander(state)]\nRightPath = /\n");
        _previousDirectory = Environment.CurrentDirectory;
        Environment.CurrentDirectory = Sandbox;
        return builder;
    }

    [TearDown]
    public void RestoreDirectory()
    {
        Environment.CurrentDirectory = _previousDirectory;
        TestDataDirectory.DeleteSettings();
    }

    [Test]
    public async Task DirectoryOfTerminalWinsOverStartupAndSavedLocations()
    {
        await UITest.WaitNoText("Processing");
        await UITest.WaitBuffer(_ => Commander.LeftPanel?.CurrentCatalog?.CurrentPath == Sandbox &&
                                     Commander.RightPanel?.CurrentCatalog?.CurrentPath == Sandbox,
            "Panels are not opened in the directory Bashka was started in");
    }
}
