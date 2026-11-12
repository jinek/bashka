# Bashka.Plugin.Template

Run `dotnet run` to launch Bashka with this plugin. The shell connection label is
yellow, and F1 opens a "Hello world" dialog using Bashka's Core dialog provider.

`PluginModule` registers commands and visual extensions with Autofac.
`PluginVisualExtension` loads `Styles.axaml`, which styles text blocks carrying
both the `Bashka` and `ShellConnectionInfo` classes.
`Program.cs` is a development entry point; the plugin loader registers the module
without invoking it.

Build a distributable plugin with `dotnet pack -c Release`. Keep the package ID
and assembly name equal, and retain the `bashka-plugin` tag for discovery.
Other plugins can reference this package through a normal `PackageReference`.

`Bashka.Plugins` supplies the shared Bashka APIs as a normal NuGet dependency.
Its runtime assets are excluded on this reference because installed plugins use
the host's shared assemblies. `Bashka.Launcher` supplies those runtime assets for
local development; `PrivateAssets="all"` prevents that development dependency
from appearing in the plugin's package dependencies.

The Bashka package versions are set when the template is packaged. To target
a different Bashka release later, update both package reference versions together.
Additional plugin dependencies should use normal package references. The host
must resolve their runtime assets and reuse its own Bashka, Autofac, and UI
assemblies when loading the plugin.
