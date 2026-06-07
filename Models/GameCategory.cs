using System;

namespace RenPyTRLauncher.Models
{
    public class GameCategory
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string Icon { get; set; } = "📁";
        public string AccentColor { get; set; } = "#3498DB";
        public int SortOrder { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
