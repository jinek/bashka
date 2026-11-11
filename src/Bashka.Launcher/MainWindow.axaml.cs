using Autofac;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using Bashka.Plugins.Core.Abstract;
using Bashka.Plugins.Core.Dialog;
using Bashka.Plugins.Core.Dialog.Abstract;
using Bashka.Plugins.Core.Scoping;
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

            Scope = Scope.BeginLifetimeScope(RegistrationHelper.Hierarchy,builder =>
            {
                builder.RegisterInstance(dialogProvider).As<IDialogProvider>();
                builder.RegisterType<UITaskFactory>().As<IUITaskFactory>().Keyed<IUITaskFactory>(TaskFactoryHelper.UITaskFactoryKey)
                    .SingleInstance();
                builder.RegisterType<ProgressPresenter>().As<IProgressPresenter>().SingleInstance();
                builder.RegisterType<WorkerTaskFactory>().As<TaskFactory>().AsSelf()
                    .SingleInstance();
            });
            var uiRoots = Scope.Resolve<IEnumerable<IUIRoot>>();
            IUIRoot iuiRoot = uiRoots.First();
            Content = iuiRoot;
        }, DispatcherPriority.Loaded);
    }

    public static ILifetimeScope Scope { get; internal set; } = null!;

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}