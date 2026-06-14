using System;

namespace RenPyTRLauncher.Models
{
    public class GameTag
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string? Color { get; set; } // Hex color code for tag styling
        public Guid GameId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
