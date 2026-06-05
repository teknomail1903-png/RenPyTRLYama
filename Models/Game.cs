using System;
using System.Collections.Generic;

namespace RenPyTRLauncher.Models
{
    public class Game
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Version { get; set; } = string.Empty;
        public string ImagePath { get; set; } = string.Empty; // relative path or URL
        public List<string> Categories { get; set; } = new();
        public bool IsVip { get; set; } = false;
        public bool IsTop10 { get; set; } = false;
        public bool IsFeatured { get; set; } = false; // shows in banner
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public int DownloadCount { get; set; } = 0;
        public string PatchFilePath { get; set; } = string.Empty;
        public string PatchVersion { get; set; } = string.Empty;
    }
}
