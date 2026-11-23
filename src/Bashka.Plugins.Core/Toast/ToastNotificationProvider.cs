using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Bashka.Plugins.Core.Toast.Abstract;

namespace Bashka.Plugins.Core.Toast;

public class ToastNotificationProvider : IToastNotificationProvider
{
    private readonly WindowNotificationManager _notificationManager;

    public ToastNotificationProvider(TopLevel host)
    {
        _notificationManager = new WindowNotificationManager(host);
        _notificationManager.ApplyTemplate();
    }

    public void ShowToast(string title, string message, NotificationType type = NotificationType.Information)
    {
        _notificationManager.Show(new Notification(title, message, type));
    }
}
