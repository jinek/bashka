using Avalonia.Controls.Notifications;

namespace Bashka.Plugins.Core.Toast.Abstract;

public interface IToastNotificationProvider
{
    void ShowToast(string title, string message, NotificationType type = NotificationType.Information);
}