using System.Text.RegularExpressions;
using Autofac;
using Bashka.Plugins.Core.Configuration;
using Bashka.Plugins.Core.Orthodox;
using Bashka.Plugins.Core.Scoping;
using PropertyChanged;

namespace Bashka.Tests;

[TestFixture]
public class SettingsTests
{
    [SetUp]
    [TearDown]
    public void DeleteSettings()
    {
        TestDataDirectory.DeleteSettings();
    }

    private const string RangerDefaults = """
                                          [OrthodoxCommander(keys)]
                                          ; find as in ranger
                                          RangerFindCommand = CTRL+G
                                          QuickSearchCommand = CTRL+S
                                          """;

    [Test]
    public void PluginUserAndStateFilesMergeAtStartup()
    {
        Directory.CreateDirectory(Settings.UserDirectory);
        File.WriteAllText(Path.Combine(Settings.UserDirectory, "10-mine.ini"), """
                                                                               [OrthodoxCommander(keys)]
                                                                               MarkAllCommand =
                                                                               InvertMarksCommand = CTRL+I
                                                                               [OrthodoxCommander]
                                                                               AltCharActivatesSearch = false
                                                                               StartupLocationLeft = %HOME%/work
                                                                               StartupLocationRight = %BASHKA_TEST%/x
                                                                               """);
        Environment.SetEnvironmentVariable("BASHKA_TEST", "/expanded");
        File.WriteAllText(Path.Combine(Settings.UserDirectory, "20-later.ini"),
            "[OrthodoxCommander(keys)]\nInvertMarksCommand = CTRL+J\n");
        File.WriteAllText(Path.Combine(Settings.UserDirectory, "30-upper.INI"),
            "[OrthodoxCommander(keys)]\nUnmarkAllCommand = CTRL+U\n");
        File.WriteAllText(Path.Combine(Settings.UserDirectory, "notes.txt"),
            "[OrthodoxCommander]\nAltCharActivatesSearch = true\n");

        using (IContainer container = Build())
        {
            Assert.That(container.Resolve<Settings>().PluginFiles.Select(file => file.ResourceName),
                Is.EqualTo(new[] { "orthodoxcommander.ini", "ranger.ini" }));
            var keyBindings = container.Resolve<OrthodoxCommanderKeyBindings>();
            Assert.That(keyBindings, Is.TypeOf<RangerKeyBindings>(), "inheritor is registered instead of the parent");
            Assert.That(Bound("CTRL+G"), Is.EqualTo(new[] { nameof(RangerKeyBindings.RangerFindCommand) }));
            Assert.That(Bound("CTRL+S"), Is.EqualTo(new[] { "QuickSearchCommand" }), "later plugin overrides");
            Assert.That(Bound("CTRL+F"), Is.Empty);
            Assert.That(Bound("SHIFT+DOWN"), Is.EqualTo(new[] { "ToggleMarkCommand" }), "several keys of one command");
            Assert.That(Bound("SHIFT+F5"), Is.EqualTo(new[] { "ShiftF5SlotCommand" }));
            Assert.That(Bound("ADD"), Is.Empty, "empty value unbinds the default");
            Assert.That(Bound("CTRL+J"), Is.EqualTo(new[] { "InvertMarksCommand" }), "user files merge in name order");
            Assert.That(Bound("CTRL+U"), Is.EqualTo(new[] { "UnmarkAllCommand" }), "extension case does not matter");
            var commanderSettings = container.Resolve<OrthodoxCommanderSettings>();
            Assert.That(commanderSettings.AltCharActivatesSearch, Is.False, "only ini files are merged");
            Assert.That(commanderSettings.StartupLocationLeft,
                Is.EqualTo(Environment.GetEnvironmentVariable("HOME") + "/work"));
            Assert.That(commanderSettings.StartupLocationRight, Is.EqualTo("/expanded/x"),
                "environment variables are expanded");

            container.Resolve<PanelState>().LeftPath = "/home";

            string[] Bound(string keys)
            {
                return [.. keyBindings.MatchCommandNames(new KeyCombination(keys))];
            }
        }

        File.WriteAllText(Path.Combine(Settings.UserDirectory, "10-mine.ini"),
            "[Panels]\nLeftPath = /user\nRightPath = /tmp\n");
        File.WriteAllText(Settings.StateFilePath, "[panels]\nleftpath = /home\n");
        using (IContainer restarted = Build())
        {
            var panels = restarted.Resolve<PanelState>();
            Assert.That(panels.LeftPath, Is.EqualTo("/home"), "state overrides user files, names ignore case");
            Assert.That(panels.RightPath, Is.EqualTo("/tmp"));
            panels.SortDescending = true;
            panels.LeftPath = "/var";
        }

        string state = File.ReadAllText(Settings.StateFilePath);
        Assert.That(Regex.Count(state, "leftpath", RegexOptions.IgnoreCase), Is.EqualTo(1),
            "state key written in another case replaces the existing one");
        Assert.That(state, Does.Contain("/var"));
        Assert.That(state, Does.Contain("SortDescending").And.Contain("True"),
            "state is written over the current file");
    }

    private static IContainer Build()
    {
        var builder = new ContainerBuilder();
        builder.RegisterType<Settings>().AsSelf().SingleInstance();
        builder.RegisterBuiltinSettingsFile(typeof(OrthodoxCommander).Assembly, "orthodoxcommander.ini");
        builder.RegisterInstance(new PluginSettingsFile(typeof(SettingsTests).Assembly, "ranger.ini", RangerDefaults));
        builder.RegisterSettings<OrthodoxCommanderSettings>();
        builder.RegisterSettings<OrthodoxCommanderKeyBindings>();
        builder.RegisterSettings<RangerKeyBindings>().As<OrthodoxCommanderKeyBindings>();
        builder.RegisterSettings<PanelState>();
        return builder.Build();
    }

    internal sealed class RangerKeyBindings : OrthodoxCommanderKeyBindings
    {
        public string? RangerFindCommand { get; init; }
    }

    [SettingsSection("Panels")]
    [AddINotifyPropertyChangedInterface]
    internal sealed class PanelState
    {
        public string LeftPath { get; set; } = "/";
        public string RightPath { get; set; } = "/";

        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public bool SortDescending { get; set; }
    }
}