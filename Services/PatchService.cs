using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using RenPyTRLauncher.Models;

namespace RenPyTRLauncher.Services
{
    public class PatchService : IPatchService
    {
        private static readonly HttpClient Http = new()
        {
            Timeout = TimeSpan.FromMinutes(10)
        };

        private readonly IGameService _gameService;
        private readonly IUserService _userService;

        public PatchService(IGameService gameService, IUserService userService)
        {
            _gameService = gameService;
            _userService = userService;
        }

        public async Task<string> ResolveDownloadUrlAsync(Game game, CancellationToken cancellationToken = default)
        {
            var source = game.PatchFilePath?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(source))
                throw new InvalidOperationException("Bu oyun için yama kaynağı tanımlı değil.");

            if (source.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                source.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                return source;

            if (source.StartsWith("github:", StringComparison.OrdinalIgnoreCase))
            {
                var parts = source.Substring("github:".Length).Split('/', StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length < 3)
                    throw new InvalidOperationException("github:owner/repo/asset.zip formatı bekleniyor.");

                var owner = parts[0];
                var repo = parts[1];
                var assetName = string.Join("/", parts.Skip(2));

                var apiUrl = $"https://api.github.com/repos/{owner}/{repo}/releases/latest";
                using var request = new HttpRequestMessage(HttpMethod.Get, apiUrl);
                request.Headers.UserAgent.ParseAdd("RenPyTRLauncher/1.0");
                using var response = await Http.SendAsync(request, cancellationToken).ConfigureAwait(false);
                response.EnsureSuccessStatusCode();

                await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
                using var doc = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken).ConfigureAwait(false);

                if (!doc.RootElement.TryGetProperty("assets", out var assets))
                    throw new InvalidOperationException("GitHub release asset listesi bulunamadı.");

                foreach (var asset in assets.EnumerateArray())
                {
                    if (!asset.TryGetProperty("name", out var nameEl)) continue;
                    var name = nameEl.GetString();
                    if (!string.Equals(name, assetName, StringComparison.OrdinalIgnoreCase)) continue;

                    if (asset.TryGetProperty("browser_download_url", out var urlEl))
                        return urlEl.GetString() ?? throw new InvalidOperationException("İndirme URL'si boş.");
                }

                throw new InvalidOperationException($"Release içinde '{assetName}' asset'i bulunamadı.");
            }

            if (File.Exists(source))
                return source;

            throw new InvalidOperationException("Geçerli bir yama URL'si veya dosya yolu bulunamadı.");
        }

        public async Task<PatchInstallResult> InstallPatchAsync(Game game, string gameRootFolder, User user, CancellationToken cancellationToken = default)
        {
            var log = new StringBuilder();
            var logDir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "RenPyTRLauncher", "Logs");
            Directory.CreateDirectory(logDir);
            var logFile = Path.Combine(logDir, $"patch_{game.Id:N}_{DateTime.Now:yyyyMMdd_HHmmss}.log");

            var tempRoot = Path.Combine(Path.GetTempPath(), "RenPyTRLauncher", Guid.NewGuid().ToString("N"));
            var backupRoot = Path.Combine(tempRoot, "backup");
            var extractRoot = Path.Combine(tempRoot, "extract");
            var rollbackEntries = new List<(string Destination, string? BackupPath)>();

            void Log(string message)
            {
                log.AppendLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}");
            }

            try
            {
                if (string.IsNullOrWhiteSpace(gameRootFolder) || !Directory.Exists(gameRootFolder))
                    throw new InvalidOperationException("Geçerli bir oyun klasörü seçilmedi.");

                Directory.CreateDirectory(tempRoot);
                Directory.CreateDirectory(extractRoot);

                Log($"Oyun: {game.Name}");
                Log($"Hedef klasör: {gameRootFolder}");

                var downloadSource = await ResolveDownloadUrlAsync(game, cancellationToken).ConfigureAwait(false);
                string zipPath;

                if (downloadSource.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                {
                    zipPath = Path.Combine(tempRoot, "patch.zip");
                    Log($"İndiriliyor: {downloadSource}");
                    using var response = await Http.GetAsync(downloadSource, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
                    response.EnsureSuccessStatusCode();
                    await using var fs = File.Create(zipPath);
                    await response.Content.CopyToAsync(fs, cancellationToken).ConfigureAwait(false);
                    Log("İndirme tamamlandı.");
                }
                else
                {
                    zipPath = downloadSource;
                    Log($"Yerel dosya kullanılıyor: {zipPath}");
                }

                Log("ZIP çıkarılıyor...");
                ZipFile.ExtractToDirectory(zipPath, extractRoot, overwriteFiles: true);

                var sourceGameDir = FindGameContentFolder(extractRoot);
                Log($"Kaynak game klasörü: {sourceGameDir}");

                var targetGameDir = Path.Combine(gameRootFolder, "game");
                if (!Directory.Exists(targetGameDir))
                {
                    Directory.CreateDirectory(targetGameDir);
                    Log("Hedef game klasörü oluşturuldu.");
                }

                var files = Directory.GetFiles(sourceGameDir, "*", SearchOption.AllDirectories);
                Log($"{files.Length} dosya kurulacak.");

                foreach (var sourceFile in files)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    var relative = Path.GetRelativePath(sourceGameDir, sourceFile);
                    var destFile = Path.Combine(targetGameDir, relative);
                    var destDir = Path.GetDirectoryName(destFile);
                    if (!string.IsNullOrEmpty(destDir))
                        Directory.CreateDirectory(destDir);

                    string? backupPath = null;
                    if (File.Exists(destFile))
                    {
                        backupPath = Path.Combine(backupRoot, relative);
                        var backupDir = Path.GetDirectoryName(backupPath);
                        if (!string.IsNullOrEmpty(backupDir))
                            Directory.CreateDirectory(backupDir);
                        File.Copy(destFile, backupPath, overwrite: true);
                    }

                    File.Copy(sourceFile, destFile, overwrite: true);
                    rollbackEntries.Add((destFile, backupPath));
                    Log($"Kuruldu: {relative}");
                }

                _gameService.IncrementDownloadCount(game.Id);
                _userService.RecordPatchDownload(user.Id, game.Id);
                ServiceLocator.NotifyDataChanged();

                Log("Kurulum başarıyla tamamlandı.");
                await File.WriteAllTextAsync(logFile, log.ToString(), cancellationToken).ConfigureAwait(false);

                return new PatchInstallResult
                {
                    Success = true,
                    Message = $"{game.Name} Türkçe yaması başarıyla kuruldu. ({files.Length} dosya)",
                    LogFilePath = logFile,
                    FilesInstalled = files.Length
                };
            }
            catch (Exception ex)
            {
                Log($"HATA: {ex.Message}");
                Rollback(rollbackEntries, Log);
                try { await File.WriteAllTextAsync(logFile, log.ToString(), cancellationToken).ConfigureAwait(false); } catch { }

                return new PatchInstallResult
                {
                    Success = false,
                    Message = $"Kurulum başarısız: {ex.Message}",
                    LogFilePath = logFile
                };
            }
            finally
            {
                try { if (Directory.Exists(tempRoot)) Directory.Delete(tempRoot, recursive: true); } catch { }
            }
        }

        private static string FindGameContentFolder(string extractRoot)
        {
            var directGame = Path.Combine(extractRoot, "game");
            if (Directory.Exists(directGame))
                return directGame;

            var nested = Directory.GetDirectories(extractRoot, "game", SearchOption.AllDirectories);
            if (nested.Length > 0)
                return nested[0];

            return extractRoot;
        }

        private static void Rollback(List<(string Destination, string? BackupPath)> entries, Action<string> log)
        {
            log("Rollback başlatılıyor...");
            foreach (var (dest, backup) in entries.AsEnumerable().Reverse())
            {
                try
                {
                    if (backup != null && File.Exists(backup))
                    {
                        var dir = Path.GetDirectoryName(dest);
                        if (!string.IsNullOrEmpty(dir))
                            Directory.CreateDirectory(dir);
                        File.Copy(backup, dest, overwrite: true);
                        log($"Geri alındı: {dest}");
                    }
                    else if (File.Exists(dest))
                    {
                        File.Delete(dest);
                        log($"Yeni dosya silindi: {dest}");
                    }
                }
                catch (Exception ex)
                {
                    log($"Rollback uyarısı ({dest}): {ex.Message}");
                }
            }
        }
    }
}
