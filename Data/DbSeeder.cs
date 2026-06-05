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
        public static readonly Guid LuckyParadoxId = Guid.Parse("a1000004-0000-4000-8000-000000000004");

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
                        Categories = new System.Collections.Generic.List<string> { "VIP", "Romance", "Erkek Başrol", "Devam Eden" },
                        IsVip = true,
                        IsTop10 = true,
                        IsFeatured = true,
                        DownloadCount = 1200,
                        PatchVersion = "v1.0",
                        PatchFilePath = "github:RenPyTR/SummerMemories/SummerMemories_TR_v1.0.zip",
                        CreatedDate = DateTime.UtcNow.AddDays(-30),
                        UpdatedDate = DateTime.UtcNow.AddDays(-2)
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
                        DownloadCount = 980,
                        PatchVersion = "v0.9.4-tr",
                        CreatedDate = DateTime.UtcNow.AddDays(-14),
                        UpdatedDate = DateTime.UtcNow.AddDays(-1)
                    },
                    new Game
                    {
                        Id = MilfyCityId,
                        Name = "Milfy City",
                        Description = "Şehir macerası görsel roman.",
                        Version = "1.0",
                        ImagePath = "Images/milfycity.jpg",
                        Categories = new System.Collections.Generic.List<string> { "Biten", "Kadın Başrol" },
                        IsTop10 = true,
                        DownloadCount = 760,
                        PatchVersion = "v1.0-tr",
                        CreatedDate = DateTime.UtcNow.AddDays(-7),
                        UpdatedDate = DateTime.UtcNow.AddDays(-3)
                    },
                    new Game
                    {
                        Id = LuckyParadoxId,
                        Name = "Lucky Paradox",
                        Description = "Macera dolu görsel roman.",
                        Version = "0.8",
                        ImagePath = "Images/luckyparadox.jpg",
                        Categories = new System.Collections.Generic.List<string> { "Devam Etmeyen", "Erkek Başrol" },
                        IsTop10 = true,
                        DownloadCount = 540,
                        PatchVersion = "v0.8-tr",
                        CreatedDate = DateTime.UtcNow.AddDays(-5),
                        UpdatedDate = DateTime.UtcNow.AddDays(-4)
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
                    MembershipLevel = "Gold",
                    Badges = new System.Collections.Generic.List<string> { "🏆 İlk 100", "💎 VIP Üye", "🔥 Aktif Kullanıcı" },
                    TotalDownloadCount = 12,
                    FavoriteGameIds = new System.Collections.Generic.List<Guid> { SummerMemoriesId, BeingADikId },
                    DownloadedPatchIds = new System.Collections.Generic.List<Guid> { SummerMemoriesId, MilfyCityId },
                    RecentDownloadedGameIds = new System.Collections.Generic.List<Guid> { BeingADikId, SummerMemoriesId, MilfyCityId }
                };
                db.Users.Add(argion);
                db.Users.Add(new User
                {
                    Username = "user1",
                    Email = "user1@example.com",
                    Role = "User",
                    MembershipLevel = "Ücretsiz"
                });
            }

            if (!db.Announcements.Any())
            {
                db.Announcements.AddRange(
                    new Announcement
                    {
                        Title = "RenPyTR Launcher'a Hoş Geldiniz!",
                        Message = "Türkçe yamaları buradan indirebilir ve kurabilirsiniz.",
                        AccentColor = "#9B59FF",
                        CreatedAt = DateTime.UtcNow
                    },
                    new Announcement
                    {
                        Title = "Yeni Yamalar Yayında!",
                        Message = "Being A DIK ve Summer Memories için güncel Türkçe yamalar eklendi.",
                        AccentColor = "#E74C3C",
                        CreatedAt = DateTime.UtcNow.AddHours(-6)
                    },
                    new Announcement
                    {
                        Title = "VIP Üyelik Avantajları",
                        Message = "VIP üyeler özel yamalara erken erişim kazanır.",
                        AccentColor = "#F1C40F",
                        CreatedAt = DateTime.UtcNow.AddHours(-12)
                    });
            }

            if (!db.MembershipTiers.Any())
            {
                db.MembershipTiers.AddRange(
                    new MembershipTier
                    {
                        Name = "Bronze Kart",
                        Icon = "🥉",
                        Price = 49,
                        PriceLabel = "49 ₺ / ay",
                        Features = new System.Collections.Generic.List<string> { "Temel VIP yamalar", "Discord rolü", "Öncelikli destek" },
                        PurchaseUrl = "https://renpytr.com/vip/bronze",
                        AccentColor = "#CD7F32",
                        SortOrder = 1
                    },
                    new MembershipTier
                    {
                        Name = "Silver Kart",
                        Icon = "🥈",
                        Price = 99,
                        PriceLabel = "99 ₺ / ay",
                        Features = new System.Collections.Generic.List<string> { "Tüm Bronze özellikler", "Erken erişim yamaları", "Özel rozet" },
                        PurchaseUrl = "https://renpytr.com/vip/silver",
                        AccentColor = "#C0C0C0",
                        SortOrder = 2
                    },
                    new MembershipTier
                    {
                        Name = "Gold Kart",
                        Icon = "🥇",
                        Price = 149,
                        PriceLabel = "149 ₺ / ay",
                        Features = new System.Collections.Generic.List<string> { "Tüm Silver özellikler", "VIP oyun kütüphanesi", "Beta yamalar" },
                        PurchaseUrl = "https://renpytr.com/vip/gold",
                        AccentColor = "#FFD700",
                        SortOrder = 3
                    },
                    new MembershipTier
                    {
                        Name = "Sınırsız Kart",
                        Icon = "♾️",
                        Price = 299,
                        PriceLabel = "299 ₺ / tek sefer",
                        Features = new System.Collections.Generic.List<string> { "Sınırsız VIP erişim", "Tüm mevcut ve gelecek yamalar", "Özel destek hattı" },
                        PurchaseUrl = "https://renpytr.com/vip/unlimited",
                        AccentColor = "#9B59FF",
                        SortOrder = 4
                    });
            }

            if (!db.AppSettings.Any())
            {
                db.AppSettings.AddRange(
                    new AppSetting { Key = AppSettingKeys.WebsiteUrl, Value = "https://renpytr.com" },
                    new AppSetting { Key = AppSettingKeys.DiscordUrl, Value = "https://discord.gg/renpytr" },
                    new AppSetting { Key = AppSettingKeys.AnnouncementsUrl, Value = "https://renpytr.com/duyurular" },
                    new AppSetting { Key = AppSettingKeys.SupportUrl, Value = "https://renpytr.com/destek" });
            }

            var summer = db.Games.Find(SummerMemoriesId);
            if (summer != null && string.IsNullOrWhiteSpace(summer.PatchFilePath))
                summer.PatchFilePath = "github:RenPyTR/SummerMemories/SummerMemories_TR_v1.0.zip";

            var argionUser = db.Users.FirstOrDefault(u => u.Username == "argion");
            if (argionUser != null && !db.UserActivities.Any(a => a.UserId == argionUser.Id))
            {
                db.UserActivities.AddRange(
                    new UserActivity { UserId = argionUser.Id, Description = "Being A DIK yaması indirildi", Icon = "⬇️", OccurredAt = DateTime.UtcNow.AddHours(-2) },
                    new UserActivity { UserId = argionUser.Id, Description = "Summer Memories favorilere eklendi", Icon = "⭐", OccurredAt = DateTime.UtcNow.AddHours(-5) },
                    new UserActivity { UserId = argionUser.Id, Description = "Gold VIP üyeliği aktif", Icon = "💎", OccurredAt = DateTime.UtcNow.AddDays(-1) });
            }

            db.SaveChanges();
        }
    }
}
