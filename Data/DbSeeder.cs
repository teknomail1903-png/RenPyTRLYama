using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using RenPyTRLauncher.Models;
using RenPyTRLauncher.Services;

namespace RenPyTRLauncher.Data
{
    public static class DbSeeder
    {
        public static readonly Guid SummerMemoriesId = Guid.Parse("a1000001-0000-4000-8000-000000000001");
        public static readonly Guid BeingADikId = Guid.Parse("a1000002-0000-4000-8000-000000000002");
        public static readonly Guid MilfyCityId = Guid.Parse("a1000003-0000-4000-8000-000000000003");
        public static readonly Guid LuckyParadoxId = Guid.Parse("a1000004-0000-4000-8000-000000000004");
        public static readonly Guid ArgionUserId = Guid.Parse("b1000001-0000-4000-8000-000000000001");
        public static readonly Guid User1Id = Guid.Parse("b1000002-0000-4000-8000-000000000002");
        public static readonly Guid ModeratorId = Guid.Parse("b1000003-0000-4000-8000-000000000003");

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
                        PatchNotes = "v1.0 — İlk Türkçe yama sürümü.\n- Ana hikaye çevirisi tamamlandı\n- Arayüz Türkçeleştirildi",
                        DownloadLinks = new System.Collections.Generic.List<string> { "Resmi Site|https://renpytr.com", "Discord|https://discord.gg/renpytr" },
                        CreatedDate = DateTime.UtcNow.AddDays(-30),
                        UpdatedDate = DateTime.UtcNow.AddDays(-2),
                        Type = GameType.Game,
                        ParentGameId = Guid.Empty
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
                        UpdatedDate = DateTime.UtcNow.AddDays(-1),
                        Type = GameType.Game,
                        ParentGameId = Guid.Empty
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
                        UpdatedDate = DateTime.UtcNow.AddDays(-3),
                        Type = GameType.Game,
                        ParentGameId = Guid.Empty
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
                        UpdatedDate = DateTime.UtcNow.AddDays(-4),
                        Type = GameType.Game,
                        ParentGameId = Guid.Empty
                    });
            }

            if (!db.Users.Any())
            {
                var argion = new User
                {
                    Id = ArgionUserId,
                    Username = "argion",
                    Email = "argion@example.com",
                    PasswordHash = Services.PasswordHasher.Hash("admin123"),
                    IsVip = true,
                    VipEndDate = DateTime.UtcNow.AddDays(30),
                    Role = UserRole.Admin,
                    MembershipLevel = "Gold",
                    City = "İstanbul",
                    Age = 25,
                    AvatarPath = Services.ImageService.GetDefaultAvatar("argion"),
                    Badges = new System.Collections.Generic.List<string> { "🏆 İlk 100", "💎 VIP Üye", "🔥 Aktif Kullanıcı", "🛡️ Admin" },
                    TotalDownloadCount = 12,
                    FavoriteGameIds = new System.Collections.Generic.List<Guid> { SummerMemoriesId, BeingADikId },
                    DownloadedPatchIds = new System.Collections.Generic.List<Guid> { SummerMemoriesId, MilfyCityId },
                    RecentDownloadedGameIds = new System.Collections.Generic.List<Guid> { BeingADikId, SummerMemoriesId, MilfyCityId }
                };
                db.Users.Add(argion);
                db.Users.Add(new User
                {
                    Id = User1Id,
                    Username = "user1",
                    Email = "user1@example.com",
                    PasswordHash = Services.PasswordHasher.Hash("user123"),
                    Role = UserRole.User,
                    MembershipLevel = "Ücretsiz",
                    AvatarPath = Services.ImageService.GetDefaultAvatar("user1")
                });

                db.Users.Add(new User
                {
                    Id = ModeratorId,
                    Username = "moderator",
                    Email = "mod@example.com",
                    PasswordHash = Services.PasswordHasher.Hash("mod123"),
                    Role = UserRole.Mod,
                    MembershipLevel = "Ücretsiz",
                    AvatarPath = Services.ImageService.GetDefaultAvatar("moderator"),
                    Badges = new System.Collections.Generic.List<string> { "🔧 Moderatör" }
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
                        IsPinned = true,
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

            EnsureDefaultPasswords(db);

            SeedCategories(db);
            SeedHelpGuides(db);

            if (!db.AppSettings.Any(s => s.Key == AppSettingKeys.Theme))
                db.AppSettings.Add(new AppSetting { Key = AppSettingKeys.Theme, Value = ThemeService.SteamDark });

            var argionUser = db.Users.FirstOrDefault(u => u.Username == "argion");
            if (argionUser != null && !db.UserActivities.Any(a => a.UserId == argionUser.Id))
            {
                db.UserActivities.AddRange(
                    new UserActivity { UserId = argionUser.Id, Description = "Being A DIK yaması indirildi", Icon = "⬇️", OccurredAt = DateTime.UtcNow.AddHours(-2) },
                    new UserActivity { UserId = argionUser.Id, Description = "Summer Memories favorilere eklendi", Icon = "⭐", OccurredAt = DateTime.UtcNow.AddHours(-5) },
                    new UserActivity { UserId = argionUser.Id, Description = "Gold VIP üyeliği aktif", Icon = "💎", OccurredAt = DateTime.UtcNow.AddDays(-1) });
            }

            try
            {
                System.Diagnostics.Debug.WriteLine("SaveChanges öncesi - ChangeTracker Entries:");
                foreach (var entry in db.ChangeTracker.Entries())
                {
                    System.Diagnostics.Debug.WriteLine($"Entity: {entry.Entity.GetType().Name}, State: {entry.State}");
                }

                Console.WriteLine("SaveChanges öncesi - ChangeTracker Entries:");
                foreach (var entry in db.ChangeTracker.Entries())
                {
                    Console.WriteLine($"Entity={entry.Entity.GetType().Name} State={entry.State}");
                }

                try
                {
                    db.SaveChanges();
                }
                catch (Exception ex)
                {
                    App.Log($"SEED ERROR: {ex.Message}");

                    if (ex.InnerException != null)
                        App.Log($"INNER ERROR: {ex.InnerException.Message}");

                    App.Log(ex.StackTrace ?? "null stack trace");

                    throw;
                }

                System.Diagnostics.Debug.WriteLine("SaveChanges başarılı");
            }
            catch (Microsoft.EntityFrameworkCore.DbUpdateException ex)
            {
                System.Diagnostics.Debug.WriteLine("DB UPDATE ERROR");
                System.Diagnostics.Debug.WriteLine(ex.Message);

                if (ex.InnerException != null)
                {
                    System.Diagnostics.Debug.WriteLine("INNER EXCEPTION:");
                    System.Diagnostics.Debug.WriteLine(ex.InnerException.Message);
                    System.Diagnostics.Debug.WriteLine(ex.InnerException.ToString());
                    App.Log($"INNER EXCEPTION: {ex.InnerException.Message}");
                    App.Log($"INNER EXCEPTION: {ex.InnerException.ToString()}");
                }

                System.Windows.MessageBox.Show($"DB UPDATE ERROR:\n{ex.Message}\n\nINNER: {ex.InnerException?.Message}\n\nINNER: {ex.InnerException?.ToString()}", "DbSeeder Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                throw;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"SaveChanges Error: {ex.Message}");
                if (ex.InnerException != null)
                {
                    System.Diagnostics.Debug.WriteLine($"Inner Exception: {ex.InnerException.Message}");
                    System.Diagnostics.Debug.WriteLine($"Inner Exception ToString: {ex.InnerException.ToString()}");
                }
                System.Diagnostics.Debug.WriteLine($"Stack Trace: {ex.ToString()}");
                System.Windows.MessageBox.Show($"SaveChanges Error: {ex.Message}\n\nInner: {ex.InnerException?.Message}", "DbSeeder Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                throw;
            }
        }

        private static void SeedCategories(AppDbContext db)
        {
            if (db.Categories.Any()) return;
            db.Categories.AddRange(
                new GameCategory { Name = "Devam Eden", DisplayName = "Devam Edenler", Icon = "📁", AccentColor = "#3498DB", SortOrder = 1 },
                new GameCategory { Name = "Biten", DisplayName = "Bitenler", Icon = "✅", AccentColor = "#27AE60", SortOrder = 2 },
                new GameCategory { Name = "Devam Etmeyen", DisplayName = "Devam Etmeyenler", Icon = "⏸", AccentColor = "#E67E22", SortOrder = 3 },
                new GameCategory { Name = "Erkek Başrol", DisplayName = "Erkek Başrol", Icon = "👨", AccentColor = "#9B59B6", SortOrder = 4 },
                new GameCategory { Name = "Kadın Başrol", DisplayName = "Kadın Başrol", Icon = "👩", AccentColor = "#E91E63", SortOrder = 5 },
                new GameCategory { Name = "VIP", DisplayName = "VIP Oyunlar", Icon = "💎", AccentColor = "#F1C40F", SortOrder = 6 },
                new GameCategory { Name = "Romance", DisplayName = "Romance", Icon = "💕", AccentColor = "#E74C3C", SortOrder = 7 });
        }

        private static void SeedHelpGuides(AppDbContext db)
        {
            if (db.HelpGuides.Any()) return;
            db.HelpGuides.AddRange(
                new HelpGuide
                {
                    Title = "Yama Nasıl Kurulur?",
                    Content = "1. Oyunlar veya oyun detay sayfasından 'Yamayı Kur' butonuna tıklayın.\n2. Oyun klasörünü seçin (game/ klasörünün bir üst dizini).\n3. Kurulum tamamlanana kadar bekleyin.\n4. Sorun olursa Destek sayfasından talep oluşturun.",
                    Type = HelpGuideType.Text,
                    SortOrder = 1
                },
                new HelpGuide
                {
                    Title = "Favorilere Nasıl Eklenir?",
                    Content = "Oyun detay sayfasındaki yıldız butonuna tıklayarak oyunu favorilerinize ekleyebilirsiniz. Favoriler sayfasından tüm favori oyunlarınıza erişebilirsiniz.",
                    Type = HelpGuideType.Text,
                    SortOrder = 2
                },
                new HelpGuide
                {
                    Title = "Yama Kurulum Videosu",
                    Content = "Adım adım yama kurulum rehberi.",
                    Type = HelpGuideType.Video,
                    VideoUrl = "https://www.youtube.com/watch?v=dQw4w9WgXcQ",
                    SortOrder = 1
                },
                new HelpGuide
                {
                    Title = "VIP üyelik ne sağlar?",
                    Content = "VIP üyeler özel yamalara erken erişim, öncelikli destek ve özel rozetler kazanır.",
                    Type = HelpGuideType.FAQ,
                    SortOrder = 1
                },
                new HelpGuide
                {
                    Title = "Yedekten geri yükleme nasıl yapılır?",
                    Content = "Ayarlar > Bakım bölümünden 'Son Yedeği Geri Al' butonunu kullanabilirsiniz.",
                    Type = HelpGuideType.FAQ,
                    SortOrder = 2
                },
                new HelpGuide
                {
                    Title = "Ren'Py Oyun Klasörü Bulucu",
                    Content = "Oyun klasörünüzü bulmak için genellikle Steam kütüphanesinden sağ tık > Yerel Dosyalara Göz At yolunu kullanın.",
                    Type = HelpGuideType.Tool,
                    SortOrder = 1
                });
        }

        private static void EnsureDefaultPasswords(AppDbContext db)
        {
            foreach (var u in db.Users.Where(u => string.IsNullOrWhiteSpace(u.PasswordHash)))
            {
                u.PasswordHash = Services.PasswordHasher.Hash(
                    u.Username.ToLower() == "argion" ? "admin123" : "user123");
                if (string.IsNullOrWhiteSpace(u.AvatarPath))
                    u.AvatarPath = Services.ImageService.GetDefaultAvatar(u.Username);
            }
        }
    }
}
