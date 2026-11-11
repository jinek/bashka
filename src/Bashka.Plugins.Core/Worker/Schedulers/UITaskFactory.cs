namespace Bashka.Plugins.Core.Worker.Schedulers;

public class UITaskFactory() : TaskFactory(new UITaskScheduler()), IUITaskFactory;