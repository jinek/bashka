# Bashka — Bash COmmander

**Alpha version.** Bashka is a cross-platform, dual-panel file manager (and more), inspired by [Far Manager](https://www.farmanager.com/). Built with [Consolonia](https://github.com/jinek/Consolonia) and [Avalonia](https://avaloniaui.net/), it brings an extensible, keyboard-driven interface to your terminal on Linux, macOS, and Windows.

<img width="1499" height="821" alt="Bashka dual-panel file manager in a terminal" src="https://github.com/user-attachments/assets/0e662ab1-4f5a-4114-ba80-bcae4b9c780b" />

## Features

- Browse files and archives side by side, and run Bash and PowerShell commands directly inside Bashka.
- Connect over SSH natively. The same file browsing plugin works with local and remote shells.
- Extend the application through plugins: catalogs, commands, connections, UI components, and more are replaceable blocks.
- Discover and install NuGet plugins through the built-in plugin store.
- Register plugin services with the Autofac IoC container and customize the UI with Avalonia XAML styles.

Bashka is in early development. Features and plugin APIs may change between releases.

## Install and run

Install the [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0), then install the tool from NuGet:

```sh
dotnet tool install --global Bashka --prerelease
```

Launch it from your terminal:

```sh
bashka
```

If the command is not found, ensure the .NET global tools directory is on your `PATH`: `$HOME/.dotnet/tools` on Linux and macOS, or `%USERPROFILE%\.dotnet\tools` on Windows. See the [.NET tool installation guide](https://learn.microsoft.com/en-us/dotnet/core/tools/dotnet-tool-install).

Update to the latest version:

```sh
dotnet tool update --global Bashka --prerelease
```

## Uninstall

```sh
dotnet tool uninstall --global Bashka
```

## Plugins and extensibility

Open the roots menu with `Alt+F1` or `Alt+F2` and choose **Plugins** to browse the plugin store. It discovers NuGet packages tagged `bashka-plugin`.

There is currently one published extension plugin on NuGet: a plugin that changes the logo. It is a small example of how plugins can customize Bashka.

To build your own plugin, start with the [plugin project template](src/Bashka.Plugin.Template/readme.md). Plugins register their services through Autofac and can provide commands, catalogs, shell connections, and visual extensions with Avalonia XAML styles.

## Build from source

From the repository root:

```sh
dotnet run --project src/Bashka.Launcher
```

Create the installable tool package:

```sh
dotnet pack src/Bashka.Tool -c Release -o ./packages
```

The `Bashka` tool package launches `Bashka.Launcher`. The launcher also remains available as a development dependency for plugin projects.
