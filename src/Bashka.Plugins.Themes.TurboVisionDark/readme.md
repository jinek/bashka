# Bashka Turbo Vision dark theme plugin

Dark theme for Bashka based on Consolonia Turbo Vision compatible theme. It replaces the built-in theme once loaded.

Install `Bashka.Plugins.Themes.TurboVisionDark` from Bashka's **Plugins** catalog (`Alt+F1` or `Alt+F2`), then restart
Bashka to load it.

For development, run from `src`:

```sh
dotnet run --project Bashka.Plugins.Themes.TurboVisionDark
```

This starts Bashka with the plugin registered. The project references the local
`Bashka.Plugins` APIs and uses `Bashka.Launcher` as a private development dependency.
It is built, packed, and published alongside Bashka, but installed separately.
