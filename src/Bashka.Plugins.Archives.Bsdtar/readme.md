# Bashka bsdtar plugin

Browse, create, and extract archives using `bsdtar` in local and remote shells.
The shell where you work with archives must have `bsdtar` installed.

Install `Bashka.Plugins.Archives.Bsdtar` from Bashka's **Plugins** catalog
(`Alt+F1` or `Alt+F2`), then restart Bashka to load it.

The plugin registers archive navigation with `Enter`, packing with `Alt+F5` or
`Shift+F1`, and extraction with `Shift+F2`. Its archive navigation takes precedence
over the built-in archive handlers when applicable.

For development, run from `src`:

```sh
dotnet run --project Bashka.Plugins.Archives.Bsdtar
```

This starts Bashka with the plugin registered. The project references the local
`Bashka.Plugins` APIs and uses `Bashka.Launcher` as a private development dependency.
It is built, packed, and published alongside Bashka, but installed separately.
