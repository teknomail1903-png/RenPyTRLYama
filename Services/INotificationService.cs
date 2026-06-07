using System;
using System.Collections.Generic;
using RenPyTRLauncher.Models;

namespace RenPyTRLauncher.Services
{
    public interface INotificationService
    {
        IEnumerable<Notification> GetForUser(Guid userId);
        int GetUnreadCount(Guid userId);
        void MarkAsRead(Guid notificationId);
        void MarkAllAsRead(Guid userId);
        void NotifyUser(Guid userId, string title, string message, NotificationType type, Guid? relatedGameId = null);
        void NotifyAllUsers(string title, string message, NotificationType type, Guid? relatedGameId = null);
        void Remove(Guid id);
    }
}
