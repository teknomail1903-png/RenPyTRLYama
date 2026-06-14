using System;
using System.Collections.Generic;

namespace RenPyTRLauncher.Models
{
    public enum GameType
    {
        Game,
        Translation,
        Gallery,
        Cheat,
        Walkthrough,
        Save,
        Extra
    }

    public enum TurkishStatus
    {
        Evet,
        Hayır,
        DevamEdiyor,
        KismiCeviri
    }

    public enum SteamStatus
    {
        Var,
        Yok
    }

    public enum UpdateStatus
    {
        Updated,
        UpdateAvailable,
        Outdated
    }

    public enum CompletionStatus
    {
        Tamamlandi,
        DevamEdiyor,
        Beklemede
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
        public TurkishStatus TurkishStatus { get; set; } = TurkishStatus.Hayır;
        public SteamStatus SteamStatus { get; set; } = SteamStatus.Yok;
        public UpdateStatus UpdateStatus { get; set; } = UpdateStatus.Updated;
        public Guid ParentGameId { get; set; } = Guid.Empty; // For patches, the parent game ID
        public List<string> Tags { get; set; } = new(); // Steam-style tags
        
        // New fields for detailed game information
        public string Developer { get; set; } = string.Empty;
        public string Publisher { get; set; } = string.Empty;
        public string GameEngine { get; set; } = string.Empty;
        public DateTime? ReleaseDate { get; set; } = null;
        public CompletionStatus CompletionStatus { get; set; } = CompletionStatus.DevamEdiyor;
        public List<string> ContentWarnings { get; set; } = new();
        public string AveragePlaytime { get; set; } = string.Empty;
        public List<string> GameGenres { get; set; } = new(); // Ren'Py, Visual Novel, Sandbox, Adult, RPG, Dating Sim
        public List<string> Platforms { get; set; } = new(); // Windows, Android, Linux, Mac
    }
}
