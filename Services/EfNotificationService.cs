using System;
using System.Collections.Generic;
using System.Linq;
using RenPyTRLauncher.Data;
using RenPyTRLauncher.Models;

namespace RenPyTRLauncher.Services
{
    public class EfNotificationService : INotificationService
    {
        private readonly AppDbContext _db;
        private readonly IUserService _userService;

        public EfNotificationService(AppDbContext db, IUserService userService)
        {
            _db = db;
            _userService = userService;
        }

        public IEnumerable<Notification> GetForUser(Guid userId) =>
            _db.Notifications
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .Take(100)
                .ToList();

        public int GetUnreadCount(Guid userId) =>
            _db.Notifications.Count(n => n.UserId == userId && !n.IsRead);

        public void MarkAsRead(Guid notificationId)
        {
            var n = _db.Notifications.Find(notificationId);
            if (n == null) return;
            n.IsRead = true;
            _db.SaveChanges();
        }

        public void MarkAllAsRead(Guid userId)
        {
            foreach (var n in _db.Notifications.Where(x => x.UserId == userId && !x.IsRead))
                n.IsRead = true;
            _db.SaveChanges();
        }

        public void NotifyUser(Guid userId, string title, string message, NotificationType type, Guid? relatedGameId = null)
        {
            _db.Notifications.Add(new Notification
            {
                UserId = userId,
                Title = title,
                Message = message,
                Type = type,
                RelatedGameId = relatedGameId,
                CreatedAt = DateTime.UtcNow
            });
            _db.SaveChanges();
        }

        public void NotifyAllUsers(string title, string message, NotificationType type, Guid? relatedGameId = null)
        {
            foreach (var user in _userService.GetAll())
                NotifyUser(user.Id, title, message, type, relatedGameId);
        }

        public void Remove(Guid id)
        {
            var ex = _db.Notifications.Find(id);
            if (ex != null)
            {
                _db.Notifications.Remove(ex);
                _db.SaveChanges();
            }
        }
    }
}
