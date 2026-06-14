using System;
using System.Threading;
using System.Threading.Tasks;
using RenPyTRLauncher.Models;

namespace RenPyTRLauncher.Services
{
    public interface IDownloadService
    {
        Task<DownloadResult> DownloadGameAsync(Game game, string savePath, IProgress<DownloadProgress> progress, CancellationToken cancellationToken = default);
        void CancelDownload(Guid downloadId);
        bool HasActiveDownloads { get; }
    }
}
