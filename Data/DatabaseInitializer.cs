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
            App.Log("[STARTUP] DatabaseInitializer Initialize Start");
            var dbPath = Path.Combine(AppContext.BaseDirectory, "renpytrlauncher.db");
            App.Log($"[STARTUP] Database path: {dbPath}");
            var db = new AppDbContext();

            try
            {
                App.Log("[STARTUP] Calling Database.Migrate()");
                db.Database.Migrate();
                App.Log("[STARTUP] Database.Migrate() completed");
            }
            catch (Exception ex)
            {
                App.Log($"[STARTUP] Database.Migrate() failed: {ex.Message}");
                try
                {
                    if (File.Exists(dbPath))
                    {
                        var backup = dbPath + ".bak_" + DateTime.Now.ToString("yyyyMMddHHmmss");
                        App.Log($"[STARTUP] Creating backup: {backup}");
                        File.Copy(dbPath, backup, overwrite: true);
                        File.Delete(dbPath);
                    }
                    db.Dispose();
                    db = new AppDbContext();
                    App.Log("[STARTUP] Retrying Database.Migrate()");
                    db.Database.Migrate();
                    App.Log("[STARTUP] Retry Database.Migrate() completed");
                }
                catch (Exception ex2)
                {
                    App.Log($"[STARTUP] Retry failed: {ex2.Message}");
                    try { db.Database.EnsureCreated(); App.Log("[STARTUP] Database.EnsureCreated() completed"); } catch { }
                }
            }

            App.Log("[STARTUP] Calling EnsureUserColumns");
            EnsureUserColumns(db);
            App.Log("[STARTUP] EnsureUserColumns completed");
            
            App.Log("[STARTUP] Calling EnsureExtendedSchema");
            EnsureExtendedSchema(db);
            App.Log("[STARTUP] EnsureExtendedSchema completed");
            
            App.Log("[STARTUP] Calling DbSeeder.SeedIfEmpty");
            DbSeeder.SeedIfEmpty(db);
            App.Log("[STARTUP] DbSeeder.SeedIfEmpty completed");
            
            App.Log("[STARTUP] DatabaseInitializer Initialize End");
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

        private static void EnsureExtendedSchema(AppDbContext db)
        {
            try
            {
                db.Database.OpenConnection();
                using var cmd = db.Database.GetDbConnection().CreateCommand();

                cmd.CommandText = @"
                    CREATE TABLE IF NOT EXISTS MembershipTiers (
                        Id TEXT NOT NULL PRIMARY KEY,
                        Name TEXT NOT NULL,
                        Icon TEXT NOT NULL DEFAULT '💎',
                        Price REAL NOT NULL DEFAULT 0,
                        PriceLabel TEXT NOT NULL DEFAULT '',
                        Features TEXT NOT NULL DEFAULT '',
                        PurchaseUrl TEXT NOT NULL DEFAULT '',
                        AccentColor TEXT NOT NULL DEFAULT '#9B59FF',
                        SortOrder INTEGER NOT NULL DEFAULT 0,
                        IsActive INTEGER NOT NULL DEFAULT 1
                    );
                    CREATE TABLE IF NOT EXISTS AppSettings (
                        Id TEXT NOT NULL PRIMARY KEY,
                        Key TEXT NOT NULL,
                        Value TEXT NOT NULL DEFAULT ''
                    );
                    CREATE TABLE IF NOT EXISTS UserActivities (
                        Id TEXT NOT NULL PRIMARY KEY,
                        UserId TEXT NOT NULL,
                        Description TEXT NOT NULL DEFAULT '',
                        Icon TEXT NOT NULL DEFAULT '🎮',
                        OccurredAt TEXT NOT NULL
                    );";
                cmd.ExecuteNonQuery();

                EnsureColumn(db, "Games", "UpdatedDate", "TEXT NOT NULL DEFAULT ''");
                EnsureColumn(db, "Announcements", "ImagePath", "TEXT NOT NULL DEFAULT ''");
                EnsureColumn(db, "Announcements", "AccentColor", "TEXT NOT NULL DEFAULT '#9B59FF'");
                EnsureColumn(db, "Announcements", "IsActive", "INTEGER NOT NULL DEFAULT 1");
                EnsureColumn(db, "Announcements", "IsPinned", "INTEGER NOT NULL DEFAULT 0");
                EnsureColumn(db, "Users", "AvatarPath", "TEXT NOT NULL DEFAULT ''");
                EnsureColumn(db, "Users", "MembershipLevel", "TEXT NOT NULL DEFAULT 'Ücretsiz'");
                EnsureColumn(db, "Users", "Badges", "TEXT NOT NULL DEFAULT ''");
                EnsureColumn(db, "Users", "PasswordHash", "TEXT NOT NULL DEFAULT ''");
                EnsureColumn(db, "Users", "City", "TEXT NOT NULL DEFAULT ''");
                EnsureColumn(db, "Users", "Age", "INTEGER");
                EnsureColumn(db, "Games", "ScreenshotPaths", "TEXT NOT NULL DEFAULT ''");
                EnsureColumn(db, "Games", "PatchNotes", "TEXT NOT NULL DEFAULT ''");
                EnsureColumn(db, "Games", "DownloadLinks", "TEXT NOT NULL DEFAULT ''");
                EnsureColumn(db, "Games", "Type", "TEXT NOT NULL DEFAULT 'Game'");
                EnsureColumn(db, "Games", "TurkishStatus", "TEXT NOT NULL DEFAULT 'English'");
                EnsureColumn(db, "Games", "SteamStatus", "TEXT NOT NULL DEFAULT 'NotAvailable'");
                EnsureColumn(db, "Games", "UpdateStatus", "TEXT NOT NULL DEFAULT 'Updated'");
                EnsureColumn(db, "Games", "ParentGameId", "TEXT NOT NULL DEFAULT ''");

                cmd.CommandText = @"
                    CREATE TABLE IF NOT EXISTS Categories (
                        Id TEXT NOT NULL PRIMARY KEY,
                        Name TEXT NOT NULL,
                        DisplayName TEXT NOT NULL DEFAULT '',
                        Icon TEXT NOT NULL DEFAULT '📁',
                        AccentColor TEXT NOT NULL DEFAULT '#3498DB',
                        SortOrder INTEGER NOT NULL DEFAULT 0,
                        IsActive INTEGER NOT NULL DEFAULT 1
                    );
                    CREATE TABLE IF NOT EXISTS Notifications (
                        Id TEXT NOT NULL PRIMARY KEY,
                        UserId TEXT NOT NULL,
                        Title TEXT NOT NULL DEFAULT '',
                        Message TEXT NOT NULL DEFAULT '',
                        Type INTEGER NOT NULL DEFAULT 0,
                        IsRead INTEGER NOT NULL DEFAULT 0,
                        CreatedAt TEXT NOT NULL,
                        RelatedGameId TEXT
                    );
                    CREATE TABLE IF NOT EXISTS HelpGuides (
                        Id TEXT NOT NULL PRIMARY KEY,
                        Title TEXT NOT NULL DEFAULT '',
                        Content TEXT NOT NULL DEFAULT '',
                        Type INTEGER NOT NULL DEFAULT 0,
                        VideoUrl TEXT NOT NULL DEFAULT '',
                        SortOrder INTEGER NOT NULL DEFAULT 0,
                        IsActive INTEGER NOT NULL DEFAULT 1
                    );
                    CREATE TABLE IF NOT EXISTS SupportTickets (
                        Id TEXT NOT NULL PRIMARY KEY,
                        UserId TEXT NOT NULL,
                        Subject TEXT NOT NULL DEFAULT '',
                        Message TEXT NOT NULL DEFAULT '',
                        Type INTEGER NOT NULL DEFAULT 0,
                        Status INTEGER NOT NULL DEFAULT 0,
                        CreatedAt TEXT NOT NULL,
                        UpdatedAt TEXT,
                        IsReadByAdmin INTEGER NOT NULL DEFAULT 0,
                        IsReadByUser INTEGER NOT NULL DEFAULT 1
                    );
                    CREATE TABLE IF NOT EXISTS SupportMessages (
                        Id TEXT NOT NULL PRIMARY KEY,
                        SupportTicketId TEXT NOT NULL,
                        UserId TEXT NOT NULL,
                        Message TEXT NOT NULL DEFAULT '',
                        IsAdmin INTEGER NOT NULL DEFAULT 0,
                        CreatedAt TEXT NOT NULL,
                        IsRead INTEGER NOT NULL DEFAULT 0
                    );";
                cmd.ExecuteNonQuery();

                using var fixDates = db.Database.GetDbConnection().CreateCommand();
                fixDates.CommandText = "UPDATE Games SET UpdatedDate = CreatedDate WHERE UpdatedDate IS NULL OR UpdatedDate = ''";
                try { fixDates.ExecuteNonQuery(); } catch { }
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

        private static void EnsureColumn(AppDbContext db, string table, string column, string definition)
        {
            using var cmd = db.Database.GetDbConnection().CreateCommand();
            cmd.CommandText = $"PRAGMA table_info({table});";
            var existing = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read()) existing.Add(reader.GetString(1));
            }
            if (existing.Contains(column)) return;

            using var alter = db.Database.GetDbConnection().CreateCommand();
            alter.CommandText = $"ALTER TABLE {table} ADD COLUMN {column} {definition}";
            alter.ExecuteNonQuery();
        }
    }
}
