using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.EntityFrameworkCore;

namespace RenPyTRLauncher.Data
{
    public static class DatabaseInitializer
    {
        public static AppDbContext Initialize()
        {
            var dbPath = Path.Combine(AppContext.BaseDirectory, "renpytrlauncher.db");
            var db = new AppDbContext();

            try
            {
                db.Database.Migrate();
            }
            catch
            {
                try
                {
                    if (File.Exists(dbPath))
                    {
                        var backup = dbPath + ".bak_" + DateTime.Now.ToString("yyyyMMddHHmmss");
                        File.Copy(dbPath, backup, overwrite: true);
                        File.Delete(dbPath);
                    }
                    db.Dispose();
                    db = new AppDbContext();
                    db.Database.Migrate();
                }
                catch
                {
                    try { db.Database.EnsureCreated(); } catch { }
                }
            }

            EnsureUserColumns(db);
            DbSeeder.SeedIfEmpty(db);
            return db;
        }

        private static void EnsureUserColumns(AppDbContext db)
        {
            try
            {
                db.Database.OpenConnection();
                using var cmd = db.Database.GetDbConnection().CreateCommand();
                cmd.CommandText = "PRAGMA table_info(Users);";
                var existing = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read()) existing.Add(reader.GetString(1));
                }

                var needed = new[] { "FavoriteGameIds", "DownloadedPatchIds", "RecentDownloadedGameIds", "TotalDownloadCount" };
                foreach (var col in needed)
                {
                    if (existing.Contains(col)) continue;

                    using var c2 = db.Database.GetDbConnection().CreateCommand();
                    c2.CommandText = string.Equals(col, "TotalDownloadCount", StringComparison.OrdinalIgnoreCase)
                        ? $"ALTER TABLE Users ADD COLUMN {col} INTEGER NOT NULL DEFAULT 0"
                        : $"ALTER TABLE Users ADD COLUMN {col} TEXT NOT NULL DEFAULT ''";
                    c2.ExecuteNonQuery();
                }
            }
            catch
            {
                // best-effort schema patch
            }
            finally
            {
                try { db.Database.CloseConnection(); } catch { }
            }
        }
    }
}
