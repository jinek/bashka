# Project Guidelines

About project: Bashka, Orthodox file manager. Everything is split to replacable blocks. Almost everything will be a plugin.
Each plugin is a separate assembly which will register itself using Autofac.

Projects: 
Bashka.Helpers - just common shared helpers.
Bashka.Launcher - register IOC, builds the UI, and runs everything.
Bashka.Plugins.Core - main plugin which spins up orthodox idea, important types:
    * IUIRoot - base of what will be shown. Instead of Orthodox can be rander like or whatever else.
    * ICatalog - brings a list of entries to show in a panel.
    * IPanel - pane on the left or right, hosts a catalog.
    * ICommand - execution unit. Can be used to copy a file, to switch between panels, to activate functional toolbar or whatever else.
    * IShellConnection - gives a possibility to connect to a local shell or to remote one and execute different commands to retrieve list of files or to execute some command.
    * Entry - item of catalog.
    * BaseObject - base for many things, like items, panels, UIElements, handlers etc
    * directory 'Scoping' - registration and resolvation in IOC

When writting code - try not to produce duplicates.

PropertyChanged notifications are automatically wired by PropertyChanged.Fody. It's instructed in build.props file.
Fody also raises notification automatically when another an another property used by current was changed.

Properties can be injected by decorating with AutoWireAttribute.

We should prefer more strict types where enough: IEnumeraable, ReadOnly collections, AsReadonly, ToReadonly, ToArray etc.
Don't do like ToList by default.

We should not do excessive comments. Instead, just, write your explainations to output. Obvious things must not be commented. 