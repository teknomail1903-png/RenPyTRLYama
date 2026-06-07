using System;

namespace RenPyTRLauncher.Models
{
    public class Announcement
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string ImagePath { get; set; } = string.Empty;
        public string AccentColor { get; set; } = "#9B59FF";
        public bool IsActive { get; set; } = true;
        public bool IsPinned { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
