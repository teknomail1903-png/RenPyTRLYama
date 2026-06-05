using Microsoft.EntityFrameworkCore;
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

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Data Source=renpytrlauncher.db",
                x => x.MigrationsAssembly("RenPyTRLauncher"));
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // store categories as semicolon separated string using ValueConverter to avoid expression tree issues
            var converter = new Microsoft.EntityFrameworkCore.Storage.ValueConversion.ValueConverter<System.Collections.Generic.List<string>, string>(
                v => string.Join(";", v ?? new System.Collections.Generic.List<string>()),
                v => (v ?? string.Empty).Split(new[] { ';' }, System.StringSplitOptions.RemoveEmptyEntries).ToList()
            );

            modelBuilder.Entity<Game>().Property(g => g.Categories).HasConversion(converter);

            // converters for storing List<Guid> as semicolon separated strings
            var guidListConverter = new Microsoft.EntityFrameworkCore.Storage.ValueConversion.ValueConverter<System.Collections.Generic.List<System.Guid>, string>(
                v => string.Join(";", (v ?? new System.Collections.Generic.List<System.Guid>()).Select(g => g.ToString())),
                v => (v ?? string.Empty).Split(new[] { ';' }, System.StringSplitOptions.RemoveEmptyEntries).Select(s => System.Guid.Parse(s)).ToList()
            );

            modelBuilder.Entity<User>().Property(u => u.FavoriteGameIds).HasConversion(guidListConverter);
            modelBuilder.Entity<User>().Property(u => u.DownloadedPatchIds).HasConversion(guidListConverter);
            modelBuilder.Entity<User>().Property(u => u.RecentDownloadedGameIds).HasConversion(guidListConverter);
        }
    }
}
