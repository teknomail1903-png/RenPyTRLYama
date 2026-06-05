using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using RenPyTRLauncher.Models;

namespace RenPyTRLauncher.Data
{
    public static class DbSeeder
    {
        public static readonly Guid SummerMemoriesId = Guid.Parse("a1000001-0000-4000-8000-000000000001");
        public static readonly Guid BeingADikId = Guid.Parse("a1000002-0000-4000-8000-000000000002");
        public static readonly Guid MilfyCityId = Guid.Parse("a1000003-0000-4000-8000-000000000003");

        public static void SeedIfEmpty(AppDbContext db)
        {
            if (!db.Games.Any())
            {
                db.Games.AddRange(
                    new Game
                    {
                        Id = SummerMemoriesId,
                        Name = "Summer Memories",
                        Description = "Yaz tatili romantik görsel roman.",
                        Version = "1.0",
                        ImagePath = "Images/summermemories.jpg",
                        Categories = new System.Collections.Generic.List<string> { "VIP", "Romance", "Erkek Başrol" },
                        IsVip = true,
                        IsTop10 = true,
                        IsFeatured = true,
                        DownloadCount = 1200,
                        PatchVersion = "v1.0",
                        PatchFilePath = "github:RenPyTR/SummerMemories/SummerMemories_TR_v1.0.zip",
                        CreatedDate = DateTime.UtcNow.AddDays(-30)
                    },
                    new Game
                    {
                        Id = BeingADikId,
                        Name = "Being A DIK",
                        Description = "Üniversite hayatı simülasyonu.",
                        Version = "0.9.4",
                        ImagePath = "Images/beingadik.jpg",
                        Categories = new System.Collections.Generic.List<string> { "Devam Eden", "Erkek Başrol" },
                        IsTop10 = true,
                        DownloadCount = 800,
                        PatchVersion = "v0.9.4-tr",
                        CreatedDate = DateTime.UtcNow.AddDays(-14)
                    },
                    new Game
                    {
                        Id = MilfyCityId,
                        Name = "Milfy City",
                        Description = "Şehir macerası görsel roman.",
                        Version = "1.0",
                        ImagePath = "Images/milfycity.jpg",
                        Categories = new System.Collections.Generic.List<string> { "Biten", "Kadın Başrol" },
                        DownloadCount = 600,
                        PatchVersion = "v1.0-tr",
                        CreatedDate = DateTime.UtcNow.AddDays(-7)
                    });
            }

            if (!db.Users.Any())
            {
                var argion = new User
                {
                    Username = "argion",
                    Email = "argion@example.com",
                    IsVip = true,
                    VipEndDate = DateTime.UtcNow.AddDays(30),
                    Role = "Admin",
                    TotalDownloadCount = 5,
                    FavoriteGameIds = new System.Collections.Generic.List<Guid> { SummerMemoriesId, BeingADikId },
                    DownloadedPatchIds = new System.Collections.Generic.List<Guid> { SummerMemoriesId, MilfyCityId },
                    RecentDownloadedGameIds = new System.Collections.Generic.List<Guid> { BeingADikId, SummerMemoriesId, MilfyCityId }
                };
                db.Users.Add(argion);
                db.Users.Add(new User
                {
                    Username = "user1",
                    Email = "user1@example.com",
                    Role = "User"
                });
            }

            if (!db.Announcements.Any())
            {
                db.Announcements.Add(new Announcement
                {
                    Title = "RenPyTR Launcher'a Hoş Geldiniz!",
                    Message = "Türkçe yamaları buradan indirebilir ve kurabilirsiniz.",
                    CreatedAt = DateTime.UtcNow
                });
            }

            var summer = db.Games.Find(SummerMemoriesId);
            if (summer != null && string.IsNullOrWhiteSpace(summer.PatchFilePath))
                summer.PatchFilePath = "github:RenPyTR/SummerMemories/SummerMemories_TR_v1.0.zip";

            db.SaveChanges();
        }
    }
}
