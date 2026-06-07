using System;

namespace RenPyTRLauncher.Models
{
    public enum NotificationType
    {
        NewPatch = 0,
        NewGame = 1,
        Announcement = 2
    }

    public class Notification
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid UserId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public NotificationType Type { get; set; }
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public Guid? RelatedGameId { get; set; }

        public string TypeIcon => Type switch
        {
            NotificationType.NewPatch => "🔧",
            NotificationType.NewGame => "🎮",
            NotificationType.Announcement => "📢",
            _ => "🔔"
        };

        public string TypeLabel => Type switch
        {
            NotificationType.NewPatch => "Yeni Yama",
            NotificationType.NewGame => "Yeni Oyun",
            NotificationType.Announcement => "Duyuru",
            _ => "Bildirim"
        };
    }
}
