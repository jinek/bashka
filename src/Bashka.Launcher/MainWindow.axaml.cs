using Autofac;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using Bashka.Plugins.Core.Abstract;
using Bashka.Plugins.Core.Dialog;
using Bashka.Plugins.Core.Dialog.Abstract;
using Bashka.Plugins.Core.Scoping;
using Bashka.Plugins.Core.Toast;
using Bashka.Plugins.Core.Toast.Abstract;
using Bashka.Plugins.Core.Worker.ProgressUI;
using Bashka.Plugins.Core.Worker.Schedulers;
using PropertyChanged;

namespace Bashka.Launcher;

[DoNotNotify]
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        Dispatcher.UIThread.Post(() =>
        {
            var dialogProvider = new DialogProvider(this);
            var toastNotificationProvider = new ToastNotificationProvider(this);

            Scope = Scope.BeginLifetimeScope(RegistrationHelper.Hierarchy, builder =>
            {
                builder.RegisterInstance(dialogProvider).As<IDialogProvider>();
                builder.RegisterInstance(toastNotificationProvider).As<IToastNotificationProvider>();
                builder.RegisterType<UITaskFactory>().As<IUITaskFactory>()
                    .Keyed<IUITaskFactory>(TaskFactoryHelper.UITaskFactoryKey)
                    .SingleInstance();
                builder.RegisterType<ProgressPresenter>().As<IProgressPresenter>().SingleInstance();
                builder.RegisterType<WorkerTaskFactory>().As<TaskFactory>().AsSelf()
                    .SingleInstance();
            });
            var uiRoots = Scope.Resolve<IEnumerable<IUIRoot>>();
            IUIRoot iuiRoot = uiRoots.First();
            Content = iuiRoot;

            foreach ((string plugin, string reason) in IncompatiblePlugins)
                toastNotificationProvider.ShowToast($"Plugin {plugin} is skipped", reason, NotificationType.Error);

            if (PluginLoadException is { } pluginLoadException)
            {
                Dispatcher.UIThread.Post(async () =>
                {
                    await Task.Delay(5000); //todo: sleep driven development, but needed to be done after loader is closed only //todo: same time it's issue that we can not report exception at random moment because another dialog is might be shown
                    throw pluginLoadException;
                });
            }
        }, DispatcherPriority.Loaded);
    }

    public static ILifetimeScope Scope { get; internal set; } = null!;

    internal static IReadOnlyList<(string Plugin, string Reason)> IncompatiblePlugins { get; set; } = [];

    internal static AggregateException? PluginLoadException { get; set; }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}