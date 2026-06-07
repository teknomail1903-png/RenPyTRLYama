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
        public DbSet<GameCategory> Categories { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<HelpGuide> HelpGuides { get; set; }

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

            modelBuilder.Entity<Game>().Property(g => g.Categories).HasConversion(stringListConverter);
            modelBuilder.Entity<Game>().Property(g => g.ScreenshotPaths).HasConversion(stringListConverter);
            modelBuilder.Entity<Game>().Property(g => g.DownloadLinks).HasConversion(stringListConverter);

            modelBuilder.Entity<User>().Property(u => u.FavoriteGameIds).HasConversion(guidListConverter);
            modelBuilder.Entity<User>().Property(u => u.DownloadedPatchIds).HasConversion(guidListConverter);
            modelBuilder.Entity<User>().Property(u => u.RecentDownloadedGameIds).HasConversion(guidListConverter);
            modelBuilder.Entity<User>().Property(u => u.Badges).HasConversion(stringListConverter);
            modelBuilder.Entity<MembershipTier>().Property(m => m.Features).HasConversion(stringListConverter);
        }
    }
}
