using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using RenPyTRLauncher.Data;

namespace RenPyTRLauncher.Migrations
{
    public class MigrationsDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
    {
        public AppDbContext CreateDbContext(string[] args)
        {
            var dbPath = System.IO.Path.Combine(System.AppContext.BaseDirectory, "renpytrlauncher.db");
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlite($"Data Source={dbPath}", x => x.MigrationsAssembly("RenPyTRLauncher"))
                .Options;
            return new AppDbContext(options);
        }
    }
}
