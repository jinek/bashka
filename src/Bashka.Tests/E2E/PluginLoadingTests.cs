using System.Reflection;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using System.Reflection.PortableExecutable;
using Avalonia;
using Bashka.Plugins.Market;
using Bashka.Tests.Helpers;
using NuGet.Packaging;
using NuGet.Versioning;

namespace Bashka.Tests.E2E;

[TestFixture]
public class PluginLoadingTests : BashkaAppTestBase
{
    [TearDown]
    public void DeletePlugins()
    {
        foreach (string plugin in new[] { OutdatedPlugin })
        foreach (string directory in new[]
                 {
                     Path.Combine(MarketHelpers.PluginsDirectory, plugin),
#pragma warning disable CA1308 //todo: what?????
                     Path.Combine(MarketHelpers.PackagesDirectory, plugin.ToLowerInvariant())
#pragma warning restore CA1308
                 })
            if (Directory.Exists(directory))
                Directory.Delete(directory, true);
    }

    private const string OutdatedPlugin = "Bashka.Test.Outdated";
    private static readonly NuGetVersion Version = new(1, 0, 0);

    protected override AppBuilder CreateAppBuilder()
    {
        DeletePlugins();
        InstallLockFile(OutdatedPlugin);
        string library = Path.Combine(
            new VersionFolderPathResolver(MarketHelpers.PackagesDirectory).GetInstallPath(OutdatedPlugin, Version),
            "lib", MarketHelpers.GetFramework().GetShortFolderName());
        Directory.CreateDirectory(library);
        WriteAssembly(Path.Combine(library, OutdatedPlugin + ".dll"), OutdatedPlugin,
            new AssemblyName("Bashka.Plugins.Core") { Version = new Version(1, 0, 3, 0) });
        return base.CreateAppBuilder();
    }

    [Test]
    public async Task IncompatiblePluginIsSkippedWithToast()
    {
        await UITest.WaitText("Bashka.Test.Outdated is");
        await UITest.WaitText(@"1\.0\.3\.0");
        await UITest.WaitNoText("Bashka.Test.Outdated is");
        await UITest.WaitNoText("Processing");
    }

    private static void InstallLockFile(string id)
    {
        string directory = Path.Combine(MarketHelpers.PluginsDirectory, id);
        Directory.CreateDirectory(directory);
        File.WriteAllText(Path.Combine(directory, "paket.lock"), $"""
                                                                  NUGET
                                                                    remote: https://api.nuget.org/v3/index.json
                                                                      {id} ({Version})
                                                                  """);
    }

    private static void WriteAssembly(string path, string name, AssemblyName reference)
    {
        var metadata = new MetadataBuilder();
        metadata.AddModule(0, metadata.GetOrAddString(name + ".dll"), metadata.GetOrAddGuid(Guid.NewGuid()), default,
            default);
        metadata.AddAssembly(metadata.GetOrAddString(name), new Version(1, 0, 0, 0), default, default, 0,
            AssemblyHashAlgorithm.None);
        metadata.AddAssemblyReference(metadata.GetOrAddString(reference.Name!), reference.Version!, default, default,
            0, default);
        metadata.AddTypeDefinition(default, default, metadata.GetOrAddString("<Module>"), default,
            MetadataTokens.FieldDefinitionHandle(1), MetadataTokens.MethodDefinitionHandle(1));
        var blob = new BlobBuilder();
        new ManagedPEBuilder(PEHeaderBuilder.CreateLibraryHeader(), new MetadataRootBuilder(metadata),
            new BlobBuilder()).Serialize(blob);
        using FileStream stream = File.Create(path);
        blob.WriteContentTo(stream);
    }
}