using System;
using System.Collections.Generic;
using System.Linq;
using RenPyTRLauncher.Models;

namespace RenPyTRLauncher.Services
{
    public class InMemoryNotificationService : INotificationService
    {
        private readonly List<Notification> _notifications = new();
        private readonly IUserService _userService;

        public InMemoryNotificationService(IUserService userService) => _userService = userService;

        public IEnumerable<Notification> GetForUser(Guid userId) =>
            _notifications.Where(n => n.UserId == userId).OrderByDescending(n => n.CreatedAt);

        public int GetUnreadCount(Guid userId) =>
            _notifications.Count(n => n.UserId == userId && !n.IsRead);

        public void MarkAsRead(Guid notificationId)
        {
            var n = _notifications.FirstOrDefault(x => x.Id == notificationId);
            if (n != null) n.IsRead = true;
        }

        public void MarkAllAsRead(Guid userId)
        {
            foreach (var n in _notifications.Where(x => x.UserId == userId && !x.IsRead))
                n.IsRead = true;
        }

        public void NotifyUser(Guid userId, string title, string message, NotificationType type, Guid? relatedGameId = null)
        {
            _notifications.Add(new Notification
            {
                UserId = userId,
                Title = title,
                Message = message,
                Type = type,
                RelatedGameId = relatedGameId
            });
        }

        public void NotifyAllUsers(string title, string message, NotificationType type, Guid? relatedGameId = null)
        {
            foreach (var user in _userService.GetAll())
                NotifyUser(user.Id, title, message, type, relatedGameId);
        }

        public void Remove(Guid id) => _notifications.RemoveAll(n => n.Id == id);
    }
}
