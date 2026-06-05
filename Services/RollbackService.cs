using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace RenPyTRLauncher.Services
{
    public static class RollbackService
    {
        private static string BackupsRoot => Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "RenPyTRLauncher", "Backups");

        public static IEnumerable<string> ListBackupFolders()
        {
            if (!Directory.Exists(BackupsRoot)) yield break;
            foreach (var gameDir in Directory.GetDirectories(BackupsRoot))
            {
                foreach (var backup in Directory.GetDirectories(gameDir).OrderByDescending(d => d))
                    yield return backup;
            }
        }

        public static (bool Success, string Message) RollbackFromFolder(string backupFolder)
        {
            var manifest = Path.Combine(backupFolder, "manifest.txt");
            if (!File.Exists(manifest))
                return (false, "manifest.txt bulunamadı. Bu yedek geri alınamaz.");

            var log = new StringBuilder();
            var restored = 0;
            try
            {
                foreach (var line in File.ReadAllLines(manifest))
                {
                    var parts = line.Split('|', 2);
                    if (parts.Length != 2) continue;

                    var dest = parts[0];
                    var backup = parts[1];

                    if (!File.Exists(backup))
                    {
                        log.AppendLine($"Atlandı (yedek yok): {dest}");
                        continue;
                    }

                    var dir = Path.GetDirectoryName(dest);
                    if (!string.IsNullOrEmpty(dir))
                        Directory.CreateDirectory(dir);

                    File.Copy(backup, dest, overwrite: true);
                    restored++;
                    log.AppendLine($"Geri yüklendi: {dest}");
                }

                return (true, $"{restored} dosya geri yüklendi.\n{backupFolder}");
            }
            catch (Exception ex)
            {
                return (false, $"Rollback hatası: {ex.Message}");
            }
        }

        public static string? GetLatestBackup()
        {
            return ListBackupFolders().FirstOrDefault();
        }
    }
}
