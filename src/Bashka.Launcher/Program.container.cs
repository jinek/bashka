using Autofac;
using Autofac.Core;
using Autofac.Extras.AttributeMetadata;
using Bashka.Plugins.Archives;
using Bashka.Plugins.Archives.Implementations;
using Bashka.Plugins.Core;
using Bashka.Plugins.Core.Abstract;
using Bashka.Plugins.Core.Abstract.Data;
using Bashka.Plugins.Core.Abstract.Surrounding;
using Bashka.Plugins.Core.Configuration;
using Bashka.Plugins.Core.FunctionKeys;
using Bashka.Plugins.Core.Orthodox;
using Bashka.Plugins.Core.Scoping;
using Bashka.Plugins.Foundation;
using Bashka.Plugins.Foundation.Configuration;
using Bashka.Plugins.Foundation.FileSystem;
using Bashka.Plugins.Foundation.ShellInteraction;
using Bashka.Plugins.Market;

namespace Bashka.Launcher;

internal static partial class Program
{
    public static void ConfigureContainer()
    {
        var builder = new ContainerBuilder();
        builder.RegisterType<CommandExecutionContext>().AsSelf();
        builder.FullRegister<RootsCatalog, ICatalog>().Keyed<ICatalog>(ICatalog.RootCatalogKey);
        builder.FullRegister<FileSystemCatalog, ICatalog>().Keyed<ICatalog>(ICatalog.DefeaultCatalogTypeKey);
        builder.RegisterEnterCommand<ShellExecuteCommand>();
        builder.RegisterEnterCommand<OpenDirectoryCommand>();
        builder.FullRegister<ZipArchiveCatalog, IArchiveCatalog>();
        builder.FullRegister<TarArchiveCatalog, IArchiveCatalog>();
        builder.FullRegister<TarGzArchiveCatalog, IArchiveCatalog>();
        builder.FullRegister<GZipArchiveCatalog, IArchiveCatalog>();
        builder.RegisterEnterCommand<OpenZipArchiveCommand>();
        builder.RegisterEnterCommand<OpenTarArchiveCommand>();
        builder.RegisterEnterCommand<OpenTarGzArchiveCommand>();
        builder.RegisterEnterCommand<OpenGZipArchiveCommand>();
        builder.RegisterEnterCommand<SwitchConnectionCommand>();


        builder.RegisterEnterCommand<GoBackCommand>();
        builder.FullRegister<CommandSelector, ICommandSelector>();
        builder.FullRegister<Panel, IPanel>();
        builder.FullRegister<OrthodoxCommander, IUIRoot>().AsSelf().InstancePerHierarchy();
        builder.FullRegister<BashExecutor, IScriptExecutor>().Keyed<IScriptExecutor>(BashExecutor.BashKey);
        builder.RegisterType<ShellSession>().AsSelf();
        builder.RegisterType<ShellSession>()
            .Keyed<ShellSession>(IShellConnection.DefaultShellConnectionKey)
            .WithParameter(ResolvedParameter.ForKeyed<IShellConnection>(IShellConnection.DefaultShellConnectionKey));
        builder.FullRegister<LocalSystemConnectionCatalog, ICatalog>();
        builder.FullRegister<LocalSystemShellConnection, IShellConnection>()
            .Keyed<IShellConnection>(IShellConnection.DefaultShellConnectionKey);
        builder.FullRegister<ConnectionsCatalog, ICatalog>();
        builder.FullRegister<SettingsCatalog, SettingsCatalog>()
            .Keyed<ICatalog>(typeof(SettingsCatalog).GetTypeKey())
            .Keyed<ICatalog>(ICatalog.SettingsCatalogKey);
        builder.RegisterType<Settings>().AsSelf().SingleInstance();
        builder.RegisterBuiltinSettingsFile(typeof(OrthodoxCommander).Assembly, "orthodoxcommander.ini");
        builder.RegisterSettings<OrthodoxCommanderSettings>();
        builder.RegisterSettings<OrthodoxCommanderKeyBindings>();
        builder.RegisterBuiltinSettingsFile(typeof(TextEditorSettings).Assembly, "texteditor.ini");
        builder.RegisterSettings<TextEditorSettings>();
        builder.FullRegister<FunctionBar, FunctionBar>().AsSelf().InstancePerHierarchy();

        foreach (FunctionPage fSlotPage in Enum.GetValues<FunctionPage>())
        foreach (FunctionKey fSlot in Enum.GetValues<FunctionKey>())
            builder.FullRegister<FunctionBarActivationCommand, ICommand>()
                .Keyed<ICommand>($"{(fSlotPage == FunctionPage.Default ?
                    string.Empty :
                    fSlotPage.ToString())}{fSlot}SlotCommand")
                .WithParameter(new TypedParameter(typeof(FunctionPage), fSlotPage))
                .WithParameter(new TypedParameter(typeof(FunctionKey), fSlot));

        builder.FullRegister<ToggleMarkCommand, ICommand>().Keyed<ICommand>("ToggleMarkCommand")
            .WithParameter(new TypedParameter(typeof(int), 1));
        builder.FullRegister<ToggleMarkCommand, ICommand>().Keyed<ICommand>("ToggleMarkUpCommand")
            .WithParameter(new TypedParameter(typeof(int), -1));
        builder.FullRegister<MarkAllCommand, ICommand>().Keyed<ICommand>("MarkAllCommand");
        builder.FullRegister<UnmarkAllCommand, ICommand>().Keyed<ICommand>("UnmarkAllCommand");
        builder.FullRegister<InvertMarksCommand, ICommand>().Keyed<ICommand>("InvertMarksCommand");
        builder.FullRegister<QuickSearchCommand, ICommand>().Keyed<ICommand>("QuickSearchCommand");
        builder.FullRegister<OpenSettingsCommand, ICommand>().Keyed<ICommand>(nameof(OpenSettingsCommand));

        builder.RegisterFCommand<AboutCatalogCommand, ICatalog>(FunctionPage.Default, FunctionKey.F1, "About")
            .Keyed<ICommand>("AboutCommand");
        builder.RegisterFCommand<HelpCatalogCommand, ICatalog>(FunctionPage.Ctrl, FunctionKey.F1, "Help")
            .Keyed<ICommand>("HelpCommand");

        builder.RegisterFCommand<ShowLeftRootsCatalogCommand, ICatalog>(FunctionPage.Alt, FunctionKey.F1, "Left");
        builder.RegisterFCommand<ShowRightRootsCatalogCommand, ICatalog>(FunctionPage.Alt, FunctionKey.F2, "Right");

        builder.RegisterFCommand<FunctionPageChangeCatalogCommand, ICatalog>(FunctionPage.Default, FunctionKey.F12,
            "[Nrm]→Ctrl");
        builder.RegisterFCommand<FunctionPageChangeCatalogCommand, ICatalog>(FunctionPage.Ctrl, FunctionKey.F12,
            "m→[Ctrl]→Al");
        builder.RegisterFCommand<FunctionPageChangeCatalogCommand, ICatalog>(FunctionPage.Alt, FunctionKey.F12,
            "rl→[Alt]→Sh");
        builder.RegisterFCommand<FunctionPageChangeCatalogCommand, ICatalog>(FunctionPage.Shift, FunctionKey.F12,
            "t→[Shft]→Nr");

        builder.RegisterFCommand<DeleteDirOrFileCommand, ShellCatalog>(FunctionPage.Default, FunctionKey.F8, "Delete",
            true);
        builder.RegisterFCommand<NavigateToMenuCommand, ICatalog>(FunctionPage.Default, FunctionKey.F9, "Menu");
        builder.RegisterFCommand<StreamReadTextCatalogCommand, IReadStreamCatalog>(FunctionPage.Default, FunctionKey.F3,
            "View");
        builder.RegisterFCommand<FileCatCommand, ShellCatalog>(FunctionPage.Default, FunctionKey.F3, "View");
        builder.RegisterFCommand<ExitApplicationCommand, ICatalog>(FunctionPage.Default, FunctionKey.F10, "Exit");
        builder.RegisterFCommand<TouchFileCommand, ShellCatalog>(FunctionPage.Shift, FunctionKey.F4, "Touch");
        builder.RegisterFCommand<EditFileCommand, IPtyExecutor>(FunctionPage.Default, FunctionKey.F4, "Edit")
            .Keyed<ICommand>(ICommand.EditCommandKey);
        builder.RegisterFCommand<CopyStreamCommand, IReadStreamCatalog>(FunctionPage.Default, FunctionKey.F5, "CopyS");
        builder.RegisterFCommand<CopyCommand, ShellCatalog>(FunctionPage.Default, FunctionKey.F5, "Copy");
        builder.RegisterFCommand<MoveCommand, ShellCatalog>(FunctionPage.Default, FunctionKey.F6, "RenMov");
        builder.RegisterFCommand<RenameCommand, ShellCatalog>(FunctionPage.Shift, FunctionKey.F6, "Rename");
        builder.RegisterFCommand<MkdirCommand, ShellCatalog>(FunctionPage.Default, FunctionKey.F7, "MkFold");
        builder.RegisterFCommand<NewConnectionCommand, ConnectionsCatalog>(FunctionPage.Shift, FunctionKey.F4,
            "NewConn");
        builder.RegisterEnterCommand<NewConnectionEntryCommand>();
        builder.RegisterFCommand<ViewPluginSettingsCommand, SettingsCatalog>(FunctionPage.Default, FunctionKey.F3,
            "View");
        builder.RegisterFCommand<EditPluginSettingsCommand, SettingsCatalog>(FunctionPage.Default, FunctionKey.F4,
            "Edit");

        builder.RegisterFCommand<SortCommand, ICatalog>(FunctionPage.Ctrl, FunctionKey.F3, "SrtName")
            .WithParameter(new TypedParameter(typeof(SortKey), SortKey.Name));
        builder.RegisterFCommand<SortCommand, ICatalog>(FunctionPage.Ctrl, FunctionKey.F4, "SrtExt")
            .WithParameter(new TypedParameter(typeof(SortKey), SortKey.Extension));
        builder.RegisterFCommand<SortCommand, ICatalog>(FunctionPage.Ctrl, FunctionKey.F5, "SrtTime")
            .WithParameter(new TypedParameter(typeof(SortKey), SortKey.TimeStamp));
        builder.RegisterFCommand<SortCommand, ICatalog>(FunctionPage.Ctrl, FunctionKey.F6, "SrtSize")
            .WithParameter(new TypedParameter(typeof(SortKey), SortKey.Size));

        builder.FullRegister<CoreVisualExtension, VisualExtension>();
        builder.FullRegister<FoundationVisualExtension, VisualExtension>();

//builder.FullRegister<VolkovCommander,IUIRoot>();

        builder.RegisterModule<AttributedMetadataModule>();
        builder.RegisterModule<MarketModule>();

        PluginLoader.LoadInstalledPlugins(builder);

        if (DevelopmentHost.PluginModule is { } pluginModule)
            builder.RegisterModule(pluginModule);

        IContainer container = builder.Build();
        MainWindow.Scope = container.BeginLifetimeScope();
    }
}