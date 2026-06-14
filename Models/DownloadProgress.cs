using System;

namespace RenPyTRLauncher.Models
{
    public enum DownloadStatus
    {
        Downloading,
        Completed,
        Failed,
        Cancelled
    }

    public class DownloadProgress
    {
        public Guid DownloadId { get; set; }
        public Guid GameId { get; set; }
        public string GameName { get; set; } = string.Empty;
        public long BytesDownloaded { get; set; }
        public long TotalBytes { get; set; }
        public double Percentage => TotalBytes > 0 ? (BytesDownloaded * 100.0 / TotalBytes) : 0;
        public DownloadStatus Status { get; set; }
        public string? ErrorMessage { get; set; }
    }

    public class DownloadResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? FilePath { get; set; }
    }
}
