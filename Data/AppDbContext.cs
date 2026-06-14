using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using RenPyTRLauncher.Models;

namespace RenPyTRLauncher.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext()
        {
        }

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Game> Games { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Announcement> Announcements { get; set; }
        public DbSet<MembershipTier> MembershipTiers { get; set; }
        public DbSet<AppSetting> AppSettings { get; set; }
        public DbSet<UserActivity> UserActivities { get; set; }
        public DbSet<SupportTicket> SupportTickets { get; set; }
        public DbSet<SupportMessage> SupportMessages { get; set; }
        public DbSet<GameCategory> Categories { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<HelpGuide> HelpGuides { get; set; }
        public DbSet<GameTag> GameTags { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                var dbPath = Path.Combine(AppContext.BaseDirectory, "renpytrlauncher.db");
                optionsBuilder.UseSqlite($"Data Source={dbPath}",
                    x => x.MigrationsAssembly("RenPyTRLauncher"));
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            var stringListConverter = new ValueConverter<List<string>, string>(
                v => EfValueConverters.StringListToDb(v),
                v => EfValueConverters.StringListFromDb(v));

            var guidListConverter = new ValueConverter<List<Guid>, string>(
                v => EfValueConverters.GuidListToDb(v),
                v => EfValueConverters.GuidListFromDb(v));

            var gameTypeConverter = new ValueConverter<GameType, string>(
                v => v.ToString(),
                v => Enum.Parse<GameType>(v));

            var turkishStatusConverter = new ValueConverter<TurkishStatus, string>(
                v => v.ToString(),
                v => Enum.Parse<TurkishStatus>(v));

            var steamStatusConverter = new ValueConverter<SteamStatus, string>(
                v => v.ToString(),
                v => Enum.Parse<SteamStatus>(v));

            var updateStatusConverter = new ValueConverter<UpdateStatus, string>(
                v => v.ToString(),
                v => Enum.Parse<UpdateStatus>(v));

            var completionStatusConverter = new ValueConverter<CompletionStatus, string>(
                v => v.ToString(),
                v => Enum.Parse<CompletionStatus>(v));

            modelBuilder.Entity<Game>().Property(g => g.Categories).HasConversion(stringListConverter);
            modelBuilder.Entity<Game>().Property(g => g.ScreenshotPaths).HasConversion(stringListConverter);
            modelBuilder.Entity<Game>().Property(g => g.DownloadLinks).HasConversion(stringListConverter);
            modelBuilder.Entity<Game>().Property(g => g.Tags).HasConversion(stringListConverter);
            modelBuilder.Entity<Game>().Property(g => g.Type).HasConversion(gameTypeConverter).HasDefaultValue(GameType.Game);
            modelBuilder.Entity<Game>().Property(g => g.TurkishStatus).HasConversion(turkishStatusConverter).HasDefaultValue(TurkishStatus.Hayır);
            modelBuilder.Entity<Game>().Property(g => g.SteamStatus).HasConversion(steamStatusConverter).HasDefaultValue(SteamStatus.Yok);
            modelBuilder.Entity<Game>().Property(g => g.UpdateStatus).HasConversion(updateStatusConverter).HasDefaultValue(UpdateStatus.Updated);
            modelBuilder.Entity<Game>().Property(g => g.CompletionStatus).HasConversion(completionStatusConverter).HasDefaultValue(CompletionStatus.DevamEdiyor);
            modelBuilder.Entity<Game>().Property(g => g.ContentWarnings).HasConversion(stringListConverter);
            modelBuilder.Entity<Game>().Property(g => g.GameGenres).HasConversion(stringListConverter);
            modelBuilder.Entity<Game>().Property(g => g.Platforms).HasConversion(stringListConverter);

            modelBuilder.Entity<User>().Property(u => u.FavoriteGameIds).HasConversion(guidListConverter);
            modelBuilder.Entity<User>().Property(u => u.DownloadedPatchIds).HasConversion(guidListConverter);
            modelBuilder.Entity<User>().Property(u => u.RecentDownloadedGameIds).HasConversion(guidListConverter);
            modelBuilder.Entity<User>().Property(u => u.Badges).HasConversion(stringListConverter);
            modelBuilder.Entity<MembershipTier>().Property(m => m.Features).HasConversion(stringListConverter);
        }
    }
}
