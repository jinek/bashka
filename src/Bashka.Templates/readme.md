# Bashka plugin template

This template creates one executable, packable project with an Autofac module,
a visual extension that colors shell connection labels yellow, and an F1 command
that displays "Hello world" through `Bashka.Plugins.Core` dialogs.

## Build the packages locally

From `src`:

```sh
dotnet pack Bashka.sln -c Debug -o "$PWD/artifacts/packages"
dotnet new install "$PWD/artifacts/packages/Bashka.Templates.1.0.0.nupkg"
dotnet new bashka-plugin -n MyCompany.Bashka.Hello -o /tmp/MyCompany.Bashka.Hello
dotnet restore /tmp/MyCompany.Bashka.Hello --source "$PWD/artifacts/packages" --source https://api.nuget.org/v3/index.json
dotnet run --project /tmp/MyCompany.Bashka.Hello --no-restore
dotnet pack /tmp/MyCompany.Bashka.Hello -c Release --no-restore
```

The packages use the repository version (`1.0.0` for a local build). Packaging
stamps the template's two Bashka package references with `$(PackageVersion)`.
General Build obtains this from `$(ProjectVersion).$(BUILD_VERSION)` and adds
the version suffix for alpha builds. Generated plugins contain ordinary version
strings, matching the Bashka packages produced by that build. Their own initial
package version remains `1.0.0`.

Debug packaging avoids the repository's existing Release warnings-as-errors
while bootstrapping the SDK.

`Bashka.Plugins` is a dependency-only meta package for Core, Foundation, Archives,
and Archives.Bsdtar; their common dependency is Bashka.Helpers. These projects
and the development launcher are packable so the generated project uses actual
NuGet references, including during local development.

## Runtime and package behavior

The project has `OutputType=Exe`, `UseAppHost=false`, and
`EnableDynamicLoading=true`. `Program.cs` starts Bashka through
`DevelopmentHost.Run`, supplying the module under development. The development
host registers that module after the built-in registrations, matching external
plugin loading. The function bar chooses the last applicable command, allowing
plugins to override built-in bindings while retaining earlier commands as fallbacks.

`Bashka.Plugins` remains a regular package dependency. Its runtime assets are
excluded from that reference; the development-only `Bashka.Launcher` reference
supplies the runtime for `dotnet run` and has `PrivateAssets=all`. The resulting
plugin package contains the plugin assembly and a normal dependency on
`Bashka.Plugins`, without a dependency on the launcher.

The plugin loader should select compatible runtime assets, load
`<PackageId>.dll`, and register its Autofac modules without invoking the assembly
entry point. It must reuse the host's shared contract, Autofac, and UI assemblies.
NuGet installation, runtime dependency resolution, and marketplace browsing are
the next stage; they are not implemented by this template.
