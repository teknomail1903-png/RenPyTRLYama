using System;
using System.Collections.Generic;

namespace RenPyTRLauncher.Models
{
    public enum GameType
    {
        Game,
        Patch
    }

    public enum TurkishStatus
    {
        TurkishSupported,
        TurkishPatchAvailable,
        English,
        TranslationInProgress
    }

    public enum SteamStatus
    {
        Available,
        ComingSoon,
        NotAvailable
    }

    public enum UpdateStatus
    {
        Updated,
        UpdateAvailable,
        Outdated
    }

    public class Game
    {
        public Guid Id { get; set; } = Guid.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Version { get; set; } = string.Empty;
        public string ImagePath { get; set; } = string.Empty; // relative path or URL
        public List<string> Categories { get; set; } = new();
        public bool IsVip { get; set; } = false;
        public bool IsTop10 { get; set; } = false;
        public bool IsFeatured { get; set; } = false; // shows in banner
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedDate { get; set; } = DateTime.UtcNow;
        public int DownloadCount { get; set; } = 0;
        public string PatchFilePath { get; set; } = string.Empty;
        public string PatchVersion { get; set; } = string.Empty;
        public List<string> ScreenshotPaths { get; set; } = new();
        public string PatchNotes { get; set; } = string.Empty;
        public List<string> DownloadLinks { get; set; } = new();
        public GameType Type { get; set; } = GameType.Game;
        public TurkishStatus TurkishStatus { get; set; } = TurkishStatus.English;
        public SteamStatus SteamStatus { get; set; } = SteamStatus.NotAvailable;
        public UpdateStatus UpdateStatus { get; set; } = UpdateStatus.Updated;
        public Guid ParentGameId { get; set; } = Guid.Empty; // For patches, the parent game ID
    }
}
