using Autofac;
using Bashka.Plugins.Core.Abstract;
using Bashka.Plugins.Core.Abstract.Data;
using Bashka.Plugins.Core.Abstract.Surrounding;
using Bashka.Plugins.Core.Configuration;
using Bashka.Plugins.Core.FunctionKeys;
using Bashka.Plugins.Core.Orthodox;
using Bashka.Plugins.Core.Scoping;

namespace Bashka.Plugins.Core;

public sealed class CoreModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterType<CommandExecutionContext>().AsSelf();
        builder.FullRegister<RootsCatalog, ICatalog>().Keyed<ICatalog>(ICatalog.RootCatalogKey);
        builder.RegisterEnterCommand<GoBackCommand>();
        builder.FullRegister<CommandSelector, ICommandSelector>();
        builder.FullRegister<Panel, IPanel>();
        builder.FullRegister<OrthodoxCommander, IUIRoot>().AsSelf().InstancePerHierarchy();
        builder.RegisterType<Settings>().AsSelf().SingleInstance();
        builder.RegisterBuiltinSettingsFile(typeof(OrthodoxCommander).Assembly, "orthodoxcommander.ini");
        builder.RegisterSettings<OrthodoxCommanderSettings>();
        builder.RegisterSettings<OrthodoxCommanderKeyBindings>();
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

        builder.RegisterFCommand<NavigateToMenuCommand, ICatalog>(FunctionPage.Default, FunctionKey.F9, "Menu");

        builder.RegisterFCommand<SortCommand, ICatalog>(FunctionPage.Ctrl, FunctionKey.F3, "SrtName")
            .WithParameter(new TypedParameter(typeof(SortKey), SortKey.Name));
        builder.RegisterFCommand<SortCommand, ICatalog>(FunctionPage.Ctrl, FunctionKey.F4, "SrtExt")
            .WithParameter(new TypedParameter(typeof(SortKey), SortKey.Extension));
        builder.RegisterFCommand<SortCommand, ICatalog>(FunctionPage.Ctrl, FunctionKey.F5, "SrtTime")
            .WithParameter(new TypedParameter(typeof(SortKey), SortKey.TimeStamp));
        builder.RegisterFCommand<SortCommand, ICatalog>(FunctionPage.Ctrl, FunctionKey.F6, "SrtSize")
            .WithParameter(new TypedParameter(typeof(SortKey), SortKey.Size));

        builder.RegisterType<TurboVisionThemeExtension>().Keyed<VisualExtension>(VisualExtension.ThemeKey);
        builder.FullRegister<CoreVisualExtension, VisualExtension>();
    }
}