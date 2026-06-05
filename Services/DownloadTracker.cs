using System.Threading;

namespace RenPyTRLauncher.Services
{
    public static class DownloadTracker
    {
        private static int _activeDownloads;

        public static int ActiveCount => Volatile.Read(ref _activeDownloads);
        public static bool HasActiveDownloads => ActiveCount > 0;

        public static void Begin() => Interlocked.Increment(ref _activeDownloads);
        public static void End() => Interlocked.Decrement(ref _activeDownloads);
    }
}
