using Autofac;
using Autofac.Core;
using Bashka.Plugins.Core.Abstract;
using Bashka.Plugins.Core.FunctionKeys;
using Bashka.Plugins.Core.Scoping;
using Bashka.Plugins.Foundation.Configuration;
using Bashka.Plugins.Foundation.FileSystem;
using Bashka.Plugins.Foundation.ShellInteraction;

namespace Bashka.Plugins.Foundation;

public sealed class FoundationModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterCatalog<FileSystemCatalog>("File system", 6).Keyed<ICatalog>(ICatalog.DefeaultCatalogTypeKey);
        builder.RegisterEnterCommand<ShellExecuteCommand>();
        builder.RegisterEnterCommand<OpenDirectoryCommand>();
        builder.RegisterEnterCommand<SwitchConnectionCommand>();

        builder.FullRegister<BashExecutor, IScriptExecutor>().Keyed<IScriptExecutor>(BashExecutor.BashKey);
        builder.RegisterType<ShellSession>().AsSelf();
        builder.RegisterType<ShellSession>()
            .Keyed<ShellSession>(IShellConnection.DefaultShellConnectionKey)
            .WithParameter(ResolvedParameter.ForKeyed<IShellConnection>(IShellConnection.DefaultShellConnectionKey));
        builder.RegisterCatalog<LocalSystemConnectionCatalog>("Local system", 1);
        builder.FullRegister<LocalSystemShellConnection, IShellConnection>()
            .Keyed<IShellConnection>(IShellConnection.DefaultShellConnectionKey);
        builder.RegisterCatalog<ConnectionsCatalog>("SSH");
        builder.RegisterCatalog<SettingsCatalog>("Settings", -1).Keyed<ICatalog>(ICatalog.SettingsCatalogKey);
        builder.RegisterBuiltinSettingsFile(typeof(TextEditorSettings).Assembly, "texteditor.ini");
        builder.RegisterSettings<TextEditorSettings>();

        builder.RegisterFCommand<AboutCatalogCommand, ICatalog>(FunctionPage.Default, FunctionKey.F1, "About")
            .Keyed<ICommand>("AboutCommand");
        builder.RegisterFCommand<HelpCatalogCommand, ICatalog>(FunctionPage.Ctrl, FunctionKey.F1, "Help")
            .Keyed<ICommand>("HelpCommand");

        builder.RegisterFCommand<DeleteDirOrFileCommand, ShellCatalog>(FunctionPage.Default, FunctionKey.F8, "Delete",
            true);
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
        builder.RegisterFCommand<ClonePluginSettingsCommand, SettingsCatalog>(FunctionPage.Default, FunctionKey.F6,
            "Clone");

        builder.FullRegister<FoundationVisualExtension, VisualExtension>();
    }
}