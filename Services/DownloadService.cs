using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using RenPyTRLauncher.Models;

namespace RenPyTRLauncher.Services
{
    public class DownloadService : IDownloadService
    {
        private static readonly HttpClient Http = new()
        {
            Timeout = TimeSpan.FromMinutes(30)
        };

        private readonly ConcurrentDictionary<Guid, CancellationTokenSource> _activeDownloads = new();

        public bool HasActiveDownloads => !_activeDownloads.IsEmpty;

        public async Task<DownloadResult> DownloadGameAsync(Game game, string savePath, IProgress<DownloadProgress> progress, CancellationToken cancellationToken = default)
        {
            var downloadId = Guid.NewGuid();
            var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            _activeDownloads[downloadId] = cts;

            try
            {
                var downloadUrl = await ResolveDownloadUrlAsync(game, cts.Token).ConfigureAwait(false);
                
                var progressInfo = new DownloadProgress
                {
                    DownloadId = downloadId,
                    GameId = game.Id,
                    GameName = game.Name,
                    Status = DownloadStatus.Downloading
                };

                progress?.Report(progressInfo);

                if (downloadUrl.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                    downloadUrl.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                {
                    var result = await DownloadFromHttpAsync(downloadUrl, savePath, progressInfo, progress, cts.Token).ConfigureAwait(false);
                    return result;
                }
                else if (File.Exists(downloadUrl))
                {
                    File.Copy(downloadUrl, savePath, overwrite: true);
                    progressInfo.Status = DownloadStatus.Completed;
                    progressInfo.BytesDownloaded = progressInfo.TotalBytes = new FileInfo(downloadUrl).Length;
                    progress?.Report(progressInfo);
                    
                    return new DownloadResult
                    {
                        Success = true,
                        Message = $"{game.Name} başarıyla indirildi.",
                        FilePath = savePath
                    };
                }
                else
                {
                    return new DownloadResult
                    {
                        Success = false,
                        Message = "Geçerli bir indirme kaynağı bulunamadı."
                    };
                }
            }
            catch (OperationCanceledException)
            {
                var progressInfo = new DownloadProgress
                {
                    DownloadId = downloadId,
                    GameId = game.Id,
                    GameName = game.Name,
                    Status = DownloadStatus.Cancelled
                };
                progress?.Report(progressInfo);
                
                return new DownloadResult
                {
                    Success = false,
                    Message = "İndirme iptal edildi."
                };
            }
            catch (Exception ex)
            {
                var progressInfo = new DownloadProgress
                {
                    DownloadId = downloadId,
                    GameId = game.Id,
                    GameName = game.Name,
                    Status = DownloadStatus.Failed,
                    ErrorMessage = ex.Message
                };
                progress?.Report(progressInfo);
                
                return new DownloadResult
                {
                    Success = false,
                    Message = $"İndirme başarısız: {ex.Message}"
                };
            }
            finally
            {
                _activeDownloads.TryRemove(downloadId, out _);
                cts.Dispose();
            }
        }

        public void CancelDownload(Guid downloadId)
        {
            if (_activeDownloads.TryGetValue(downloadId, out var cts))
            {
                cts.Cancel();
            }
        }

        private async Task<string> ResolveDownloadUrlAsync(Game game, CancellationToken cancellationToken)
        {
            var source = game.DownloadLinks?.FirstOrDefault() ?? string.Empty;
            
            if (string.IsNullOrWhiteSpace(source))
                throw new InvalidOperationException("Bu oyun için indirme kaynağı tanımlı değil.");

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

            throw new InvalidOperationException("Geçerli bir indirme URL'si veya dosya yolu bulunamadı.");
        }

        private async Task<DownloadResult> DownloadFromHttpAsync(string url, string savePath, DownloadProgress progressInfo, IProgress<DownloadProgress>? progress, CancellationToken cancellationToken)
        {
            var directory = Path.GetDirectoryName(savePath);
            if (!string.IsNullOrEmpty(directory))
                Directory.CreateDirectory(directory);

            using var response = await Http.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();

            var totalBytes = response.Content.Headers.ContentLength ?? 0;
            progressInfo.TotalBytes = totalBytes;
            progress?.Report(progressInfo);

            await using var contentStream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
            await using var fileStream = new FileStream(savePath, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true);

            var buffer = new byte[8192];
            var bytesRead = 0;
            var totalRead = 0L;

            while ((bytesRead = await contentStream.ReadAsync(buffer, 0, buffer.Length, cancellationToken).ConfigureAwait(false)) > 0)
            {
                await fileStream.WriteAsync(buffer, 0, bytesRead, cancellationToken).ConfigureAwait(false);
                totalRead += bytesRead;

                progressInfo.BytesDownloaded = totalRead;
                progress?.Report(progressInfo);
            }

            progressInfo.Status = DownloadStatus.Completed;
            progress?.Report(progressInfo);

            return new DownloadResult
            {
                Success = true,
                Message = $"{progressInfo.GameName} başarıyla indirildi.",
                FilePath = savePath
            };
        }
    }
}
