using System;

namespace RenPyTRLauncher.Models
{
    public class User
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty; // store hash
        public bool IsVip { get; set; } = false;
        public DateTime? VipEndDate { get; set; }
        public DateTime RegisterDate { get; set; } = DateTime.UtcNow;
        public string Role { get; set; } = "User"; // User or Admin
        // Yeni profil alanlari
        public System.Collections.Generic.List<Guid> FavoriteGameIds { get; set; } = new();
        public System.Collections.Generic.List<Guid> DownloadedPatchIds { get; set; } = new();
        public System.Collections.Generic.List<Guid> RecentDownloadedGameIds { get; set; } = new();
        public int TotalDownloadCount { get; set; } = 0;
    }
}
