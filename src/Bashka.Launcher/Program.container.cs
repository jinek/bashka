using Autofac;
using Autofac.Extras.AttributeMetadata;
using Bashka.Plugins.Archives;
using Bashka.Plugins.Archives.Bsdtar;
using Bashka.Plugins.Archives.Implementations;
using Bashka.Plugins.Core;
using Bashka.Plugins.Core.Abstract;
using Bashka.Plugins.Core.Abstract.Data;
using Bashka.Plugins.Core.Abstract.Surrounding;
using Bashka.Plugins.Core.FunctionKeys;
using Bashka.Plugins.Core.Orthodox;
using Bashka.Plugins.Core.Scoping;
using Bashka.Plugins.Foundation;
using Bashka.Plugins.Foundation.FileSystem;
using Bashka.Plugins.Foundation.ShellInteraction;

namespace Bashka.Launcher;

internal static partial class Program
{
    public static void ConfigureContainer()
    {
        var builder = new ContainerBuilder();
        builder.RegisterType<CommandExecutionContext>().AsSelf();
        builder.FullRegister<RootsCatalog, ICatalog>().Keyed<ICatalog>(ICatalog.RootCatalogKey);
        builder.FullRegister<FileSystemCatalog, ICatalog>().Keyed<ICatalog>(ICatalog.DefeaultCatalogTypeKey).AsSelf();
        builder.RegisterEnterCommand<ShellExecuteCommand>();
        builder.RegisterEnterCommand<OpenDirectoryCommand>();
        builder.FullRegister<ZipArchiveCatalog, IArchiveCatalog>().AsSelf();
        builder.FullRegister<TarArchiveCatalog, IArchiveCatalog>().AsSelf();
        builder.FullRegister<TarGzArchiveCatalog, IArchiveCatalog>().AsSelf();
        builder.FullRegister<GZipArchiveCatalog, IArchiveCatalog>().AsSelf();
        builder.FullRegister<BsdtarArchiveCatalog, IReadStreamCatalog>().AsSelf();
        builder.RegisterEnterCommand<OpenZipArchiveCommand>();
        builder.RegisterEnterCommand<OpenTarArchiveCommand>();
        builder.RegisterEnterCommand<OpenTarGzArchiveCommand>();
        builder.RegisterEnterCommand<OpenGZipArchiveCommand>();
//registered after the native archive commands, so it overrides them
        builder.RegisterEnterCommand<OpenBsdtarArchiveCommand>();
        builder.RegisterEnterCommand<SwitchConnectionCommand>();


        builder.RegisterEnterCommand<GoBackCommand>();
        builder.FullRegister<CommandSelector, ICommandSelector>();
        builder.FullRegister<Panel, IPanel>();
        builder.FullRegister<OrthodoxCommander, IUIRoot>().AsSelf().InstancePerHierarchy();
        builder.FullRegister<BashExecutor, IScriptExecutor>().Keyed<IScriptExecutor>(BashExecutor.BashKey);
        builder.RegisterType<ShellSession>().AsSelf();
        builder.FullRegister<LocalSystemConnectionCatalog, ICatalog>();
        builder.FullRegister<LocalSystemShellConnection, IShellConnection>().Keyed<IShellConnection>(IShellConnection.DefaultShellConnectionKey);
        builder.FullRegister<ConnectionsCatalog, ICatalog>();
        builder.RegisterType<KeyBindingService>().AsSelf().InstancePerHierarchy();
        builder.FullRegister<FunctionBar,FunctionBar>().AsSelf().InstancePerHierarchy();

        foreach (FunctionPage fSlotPage in Enum.GetValues<FunctionPage>())
        {
            foreach (FunctionKey fSlot in Enum.GetValues<FunctionKey>())
            {
                builder.FullRegister<FunctionBarActivationCommand, ICommand>()
                    .Keyed<ICommand>($"{(fSlotPage == FunctionPage.Default ? 
                        string.Empty : 
                        fSlotPage.ToString())}{fSlot}SlotCommand")
                    .WithParameter(new TypedParameter(typeof(FunctionPage), fSlotPage))
                    .WithParameter(new TypedParameter(typeof(FunctionKey), fSlot));
            }
        }

        builder.FullRegister<ToggleMarkCommand, ICommand>().Keyed<ICommand>("ToggleMarkCommand")
            .WithParameter(new TypedParameter(typeof(int), 1));
        builder.FullRegister<ToggleMarkCommand, ICommand>().Keyed<ICommand>("ToggleMarkUpCommand")
            .WithParameter(new TypedParameter(typeof(int), -1));
        builder.FullRegister<MarkAllCommand, ICommand>().Keyed<ICommand>("MarkAllCommand");
        builder.FullRegister<UnmarkAllCommand, ICommand>().Keyed<ICommand>("UnmarkAllCommand");
        builder.FullRegister<InvertMarksCommand, ICommand>().Keyed<ICommand>("InvertMarksCommand");
        builder.FullRegister<QuickSearchCommand, ICommand>().Keyed<ICommand>("QuickSearchCommand");

        builder.RegisterFCommand<AboutCatalogCommand, ICatalog>(FunctionPage.Default, FunctionKey.F1, "About").Keyed<ICommand>("AboutCommand");
        builder.RegisterFCommand<HelpCatalogCommand, ICatalog>(FunctionPage.Ctrl, FunctionKey.F1, "Help").Keyed<ICommand>("HelpCommand");

        builder.RegisterFCommand<ShowLeftRootsCatalogCommand, ICatalog>(FunctionPage.Alt, FunctionKey.F1, "Left");
        builder.RegisterFCommand<ShowRightRootsCatalogCommand, ICatalog>(FunctionPage.Alt, FunctionKey.F2, "Right");

        builder.RegisterFCommand<FunctionPageChangeCatalogCommand, ICatalog>(FunctionPage.Default, FunctionKey.F12, "[Nrm]→Ctrl");
        builder.RegisterFCommand<FunctionPageChangeCatalogCommand, ICatalog>(FunctionPage.Ctrl, FunctionKey.F12, "m→[Ctrl]→Al");
        builder.RegisterFCommand<FunctionPageChangeCatalogCommand, ICatalog>(FunctionPage.Alt, FunctionKey.F12, "rl→[Alt]→Sh");
        builder.RegisterFCommand<FunctionPageChangeCatalogCommand, ICatalog>(FunctionPage.Shift, FunctionKey.F12, "t→[Shft]→Nr");

        builder.RegisterFCommand<DeleteDirOrFileCommand, ShellCatalog>(FunctionPage.Default, FunctionKey.F8, "Delete", true);
        builder.RegisterFCommand<NavigateToMenuCommand, ICatalog>(FunctionPage.Default, FunctionKey.F9, "Menu");
        builder.RegisterFCommand<StreamReadTextCatalogCommand, IReadStreamCatalog>(FunctionPage.Default, FunctionKey.F3, "View");
        builder.RegisterFCommand<FileCatCommand, ShellCatalog>(FunctionPage.Default, FunctionKey.F3, "View");
        builder.RegisterFCommand<ExitApplicationCommand, ICatalog>(FunctionPage.Default, FunctionKey.F10, "Exit");
        builder.RegisterFCommand<TouchFileCommand, ShellCatalog>(FunctionPage.Shift, FunctionKey.F4, "Touch");
        builder.RegisterFCommand<CopyStreamCommand, IReadStreamCatalog>(FunctionPage.Default, FunctionKey.F5, "CopyS");
        builder.RegisterFCommand<CopyCommand, ShellCatalog>(FunctionPage.Default, FunctionKey.F5, "Copy");
        builder.RegisterFCommand<MoveCommand, ShellCatalog>(FunctionPage.Default, FunctionKey.F6, "RenMov");
        builder.RegisterFCommand<RenameCommand, ShellCatalog>(FunctionPage.Shift, FunctionKey.F6, "Rename");
        builder.RegisterFCommand<MkdirCommand, ShellCatalog>(FunctionPage.Default, FunctionKey.F7, "MkFold");
        builder.RegisterFCommand<NewConnectionCommand, ConnectionsCatalog>(FunctionPage.Shift, FunctionKey.F4, "NewConn");
        builder.RegisterEnterCommand<NewConnectionEntryCommand>();
        builder.RegisterFCommand<NanoCatalogCommand, ICatalog>(FunctionPage.Default, FunctionKey.F4, "Nano");

        builder.RegisterFCommand<SortCommand, ICatalog>(FunctionPage.Ctrl, FunctionKey.F3, "SrtName")
            .WithParameter(new TypedParameter(typeof(SortKey), SortKey.Name));
        builder.RegisterFCommand<SortCommand, ICatalog>(FunctionPage.Ctrl, FunctionKey.F4, "SrtExt")
            .WithParameter(new TypedParameter(typeof(SortKey), SortKey.Extension));
        builder.RegisterFCommand<SortCommand, ICatalog>(FunctionPage.Ctrl, FunctionKey.F5, "SrtTime")
            .WithParameter(new TypedParameter(typeof(SortKey), SortKey.TimeStamp));
        builder.RegisterFCommand<SortCommand, ICatalog>(FunctionPage.Ctrl, FunctionKey.F6, "SrtSize")
            .WithParameter(new TypedParameter(typeof(SortKey), SortKey.Size));

        builder.RegisterFCommand<AddToBsdtarArchiveCommand, ShellCatalog>(FunctionPage.Alt, FunctionKey.F5, "Pack");
        builder.RegisterFCommand<AddToBsdtarArchiveCommand, ShellCatalog>(FunctionPage.Shift, FunctionKey.F1, "Pack");
        builder.RegisterFCommand<ExtractBsdtarArchiveCommand, ShellCatalog>(FunctionPage.Shift, FunctionKey.F2, "UnPack");
        builder.RegisterFCommand<ExtractBsdtarEntriesCommand, BsdtarArchiveCatalog>(FunctionPage.Shift, FunctionKey.F2, "UnPack");

        builder.FullRegister<CoreVisualExtension, VisualExtension>();
        builder.FullRegister<FoundationVisualExtension, VisualExtension>();

//builder.FullRegister<VolkovCommander,IUIRoot>();

        builder.RegisterModule<AttributedMetadataModule>();
        if (DevelopmentHost.PluginModule is { } pluginModule)
            builder.RegisterModule(pluginModule);

        IContainer container = builder.Build();
        MainWindow.Scope = container.BeginLifetimeScope();
    }


}