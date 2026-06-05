using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using RenPyTRLauncher.Data;

namespace RenPyTRLauncher.Migrations
{
    public class MigrationsDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
    {
        public AppDbContext CreateDbContext(string[] args)
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlite("Data Source=renpytrlauncher.db", x => x.MigrationsAssembly("RenPyTRLauncher"))
                .Options;
            return new AppDbContext(options);
        }
    }
}
