using System;

namespace RenPyTRLauncher.Models
{
    public class UserActivity
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid UserId { get; set; }
        public string Description { get; set; } = string.Empty;
        public string Icon { get; set; } = "🎮";
        public DateTime OccurredAt { get; set; } = DateTime.UtcNow;
    }
}
