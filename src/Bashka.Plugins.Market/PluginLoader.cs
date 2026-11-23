using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using System.Runtime.InteropServices;
using System.Runtime.Loader;
using System.Text.Json;
using Autofac;
using Autofac.Core;
using NuGet.Frameworks;
using NuGet.Packaging;
using NuGet.Versioning;
using Paket;

namespace Bashka.Plugins.Market;

public static class PluginLoader
{
    [SuppressMessage("Design", "CA1031", Justification = "Exceptions will be re-thrown")]
    public static void LoadInstalledPlugins(ContainerBuilder builder, 
        Action<string, string> onIncompatiblePlugin,
        Action<Exception> onPluginFailed)
    {
        if (!Directory.Exists(MarketHelpers.PluginsDirectory)) return;

        NuGetFramework framework = MarketHelpers.GetFramework();
        string[] runtimeIds = GetRuntimeIdentifiers();
        var paths = new VersionFolderPathResolver(MarketHelpers.PackagesDirectory);
        HashSet<string> hostAssemblies = ((string?)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES") ?? string.Empty)
            .Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries)
            .Select(path => Path.GetFileNameWithoutExtension(path))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        // todo: this is vibe-code. It's so bad I stopped at this line, please, continue/ First of all it has same code as Plugin Catalog
        var contexts = new Dictionary<string, PluginLoadContext>(StringComparer.OrdinalIgnoreCase);

        foreach (string directory in Directory.EnumerateDirectories(MarketHelpers.PluginsDirectory)
                     .Order(StringComparer.OrdinalIgnoreCase))
        {
            string lockFile = Path.Combine(directory, "paket.lock");
            if (!File.Exists(lockFile)) continue;
            string id = Path.GetFileName(directory);
            try
            {
                PackageResolver.ResolvedPackage[] packages =
                [
                    .. LockFile.LoadFrom(lockFile)
                        .GetGroup(Constants.MainDependencyGroup).Resolution.Values
                ];
                PackageResolver.ResolvedPackage root = packages.SingleOrDefault(package =>
                                                           StringComparer.OrdinalIgnoreCase.Equals(package.Name.Name, id))
                                                       ?? throw new InvalidDataException(
                                                           $"Plugin {id} is missing from {lockFile}.");
                var assemblies = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                var nativeLibraries = new Dictionary<string, string>(OperatingSystem.IsWindows()
                    ? StringComparer.OrdinalIgnoreCase
                    : StringComparer.Ordinal);
                string? entryPath = null;
                foreach (PackageResolver.ResolvedPackage package in packages)
                {
                    string packageDirectory =
                        paths.GetInstallPath(package.Name.Name, NuGetVersion.Parse(package.Version.ToString()));
                    if (!Directory.Exists(packageDirectory))
                        throw new DirectoryNotFoundException(
                            $"Plugin {id} requires missing package {package.Name.Name} {package.Version}: {packageDirectory}");

                    var packageAssemblies = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                    AddManagedFiles(Path.Combine(packageDirectory, "lib"));
                    foreach (string runtimeId in runtimeIds.Reverse())
                    {
                        string runtimeDirectory = Path.Combine(packageDirectory, "runtimes", runtimeId);
                        AddManagedFiles(Path.Combine(runtimeDirectory, "lib"));
                        string nativeDirectory = Path.Combine(runtimeDirectory, "native");
                        if (Directory.Exists(nativeDirectory))
                            foreach (string file in Directory.EnumerateFiles(nativeDirectory, "*",
                                         SearchOption.AllDirectories))
                                nativeLibraries[Path.GetFileName(file)] = file;
                    }

                    foreach (KeyValuePair<string, string> assembly in packageAssemblies)
                        if (!assemblies.TryAdd(assembly.Key, assembly.Value))
                            throw new InvalidDataException(
                                $"Plugin {id} has multiple runtime assemblies named {assembly.Key}.");
                    if (package.Equals(root))
                        entryPath = packageAssemblies.Values.SingleOrDefault(file =>
                            Path.GetFileName(file).Equals($"{id}.dll", StringComparison.OrdinalIgnoreCase));
                    continue;

                    void AddManagedFiles(string libraryDirectory)
                    {
                        foreach (string file in GetFrameworkFiles(libraryDirectory, framework))
                        {
                            var name = AssemblyName.GetAssemblyName(file);
                            packageAssemblies[AssemblyKey(name)] = file;
                        }
                    }
                }

                if (entryPath == null)
                    throw new FileNotFoundException(
                        $"No compatible {id}.dll was found for {framework.GetShortFolderName()} / {RuntimeInformation.RuntimeIdentifier}.");
                string? incompatibility = assemblies.Values
                    .Select(assembly => FindIncompatibility(id, assembly, hostAssemblies))
                    .FirstOrDefault(reason => reason != null);
                if (incompatibility != null)
                {
                    onIncompatiblePlugin(id, incompatibility);
                    continue;
                }

                string assemblyName = AssemblyName.GetAssemblyName(entryPath).Name!;
                contexts.Add(assemblyName,
                    new PluginLoadContext(id, entryPath, assemblies, nativeLibraries, hostAssemblies, contexts));
            }
            catch (Exception exception)
            {
                onPluginFailed(new InvalidOperationException($"Plugin '{id}' failed to load.", exception));
            }
        }

        var registered = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);
        foreach (string name in contexts.Keys) Register(name);

        bool Register(string name)
        {
            if (registered.TryGetValue(name, out bool isRegistered)) return isRegistered;
            registered[name] = false;
            try
            {
                Assembly assembly = contexts[name].LoadEntryAssembly();
                foreach (AssemblyName reference in assembly.GetReferencedAssemblies())
                    if (reference.Name != null && contexts.ContainsKey(reference.Name) && !Register(reference.Name))
                        throw new FileLoadException(
                            $"Plugin '{name}' depends on plugin '{reference.Name}' which was skipped.");

                IModule[] modules =
                [
                    .. assembly.GetTypes()
                        .Where(type => type is { IsAbstract: false, IsClass: true } &&
                                       typeof(IModule).IsAssignableFrom(type))
                        .Select(type => (IModule)Activator.CreateInstance(type)!)
                ];
                foreach (IModule module in modules)
                    builder.RegisterCallback(registry =>
                    {
                        try
                        {
                            module.Configure(registry);
                        }
                        catch (Exception exception)
                        {
                            onPluginFailed(
                                new InvalidOperationException($"Plugin '{name}' failed to load.", exception));
                        }
                    });
                return registered[name] = true;
            }
            catch (Exception exception)
            {
                onPluginFailed(new InvalidOperationException($"Plugin '{name}' failed to load.", exception));
                return false;
            }
        }
    }

    private static string? FindIncompatibility(string id, string assemblyPath, IReadOnlySet<string> hostAssemblies)
    {
        using var peReader = new PEReader(File.OpenRead(assemblyPath));
        MetadataReader metadata = peReader.GetMetadataReader();
        return metadata.AssemblyReferences
            .Select(handle => metadata.GetAssemblyReference(handle).GetAssemblyName())
            .Where(reference => IsBashkaHostAssembly(reference, hostAssemblies))
            .Select(reference => LoadBashkaHostAssembly(id, reference).Incompatibility)
            .FirstOrDefault(reason => reason != null);
    }

    private static bool IsBashkaHostAssembly(AssemblyName assemblyName, IReadOnlySet<string> hostAssemblies)
    {
        return assemblyName.Name != null && hostAssemblies.Contains(assemblyName.Name) &&
               assemblyName.Name.StartsWith("Bashka.", StringComparison.OrdinalIgnoreCase);
    }

    private static (Assembly Host, string? Incompatibility) LoadBashkaHostAssembly(string id,
        AssemblyName assemblyName)
    {
        var hostIdentity = (AssemblyName)assemblyName.Clone();
        hostIdentity.Version = null;
        Assembly host = AssemblyLoadContext.Default.LoadFromAssemblyName(hostIdentity);
        Version? available = host.GetName().Version;
        return assemblyName.Version is { } required &&
               (available == null || available.Major != required.Major || available.Minor < required.Minor)
            ? (host, $"Plugin '{id}' requires Bashka assembly '{assemblyName.Name}' version {required}, " +
                     $"but the host provides {available?.ToString() ?? "an unknown version"}.")
            : (host, null);
    }

    private static IEnumerable<string> GetFrameworkFiles(string directory, NuGetFramework framework)
    {
        if (!Directory.Exists(directory)) return [];
        (NuGetFramework Framework, string Path)[] groups = Directory.EnumerateDirectories(directory).Select(path =>
            (Framework: NuGetFramework.ParseFolder(Path.GetFileName(path)), Path: path)).ToArray();
        NuGetFramework? nearest = new FrameworkReducer().GetNearest(framework, groups.Select(group => group.Framework));
        string? selected = groups.FirstOrDefault(group => group.Framework.Equals(nearest)).Path;
        return selected == null
            ? Directory.EnumerateFiles(directory, "*.dll")
            : Directory.EnumerateFiles(selected, "*.dll", SearchOption.AllDirectories);
    }

    private static string[] GetRuntimeIdentifiers()
    {
        using Stream stream =
            typeof(PluginLoader).Assembly.GetManifestResourceStream("Bashka.Plugins.Market.RuntimeIdentifierGraph.json")
            !;
        using JsonDocument graph = JsonDocument.Parse(stream);
        JsonElement runtimes = graph.RootElement.GetProperty("runtimes");
        var result = new List<string>();
        var pending = new Queue<string>();
        pending.Enqueue(RuntimeInformation.RuntimeIdentifier);
        while (pending.TryDequeue(out string? runtimeId))
        {
            if (result.Contains(runtimeId, StringComparer.Ordinal)) continue;
            result.Add(runtimeId);
            if (runtimes.TryGetProperty(runtimeId, out JsonElement runtime) &&
                runtime.TryGetProperty("#import", out JsonElement imports))
                foreach (JsonElement import in imports.EnumerateArray())
                    pending.Enqueue(import.GetString()!);
        }

        return [.. result];
    }

    private static string AssemblyKey(AssemblyName name)
    {
        return $"{name.Name}/{name.CultureName}";
    }

    private sealed class PluginLoadContext(
        string id,
        string entryPath,
        IReadOnlyDictionary<string, string> assemblies,
        IReadOnlyDictionary<string, string> nativeLibraries,
        IReadOnlySet<string> hostAssemblies,
        IReadOnlyDictionary<string, PluginLoadContext> plugins) : AssemblyLoadContext($"Bashka plugin: {id}")
    {
        public Assembly LoadEntryAssembly()
        {
            return LoadFromAssemblyPath(entryPath);
        }

        protected override Assembly? Load(AssemblyName assemblyName)
        {
            if (IsBashkaHostAssembly(assemblyName, hostAssemblies))
            {
                (Assembly host, string? incompatibility) = LoadBashkaHostAssembly(id, assemblyName);
                return incompatibility == null ? host : throw new FileLoadException(incompatibility);
            }

            if (assemblyName.Name != null && hostAssemblies.Contains(assemblyName.Name))
                return Default.LoadFromAssemblyName(assemblyName);

            if (assemblyName.Name != null && plugins.TryGetValue(assemblyName.Name, out PluginLoadContext? plugin) &&
                plugin != this)
                return plugin.LoadEntryAssembly();
            return assemblies.TryGetValue(AssemblyKey(assemblyName), out string? path)
                ? LoadFromAssemblyPath(path)
                : null;
        }

        protected override nint LoadUnmanagedDll(string unmanagedDllName)
        {
            string[] names = OperatingSystem.IsWindows()
                ? [unmanagedDllName, $"{unmanagedDllName}.dll"]
                : OperatingSystem.IsMacOS()
                    ?
                    [
                        unmanagedDllName, $"lib{unmanagedDllName}.dylib", $"{unmanagedDllName}.dylib"
                    ]
                    : [unmanagedDllName, $"lib{unmanagedDllName}.so", $"{unmanagedDllName}.so"];
            foreach (string name in names)
                if (nativeLibraries.TryGetValue(name, out string? path))
                    return LoadUnmanagedDllFromPath(path);
            return nint.Zero;
        }
    }
}