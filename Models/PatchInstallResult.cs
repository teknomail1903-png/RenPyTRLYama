namespace RenPyTRLauncher.Models
{
    public class PatchInstallResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? LogFilePath { get; set; }
        public int FilesInstalled { get; set; }
    }
}
