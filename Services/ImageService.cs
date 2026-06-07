using System;
using System.IO;
using System.Linq;

namespace RenPyTRLauncher.Services
{
    public enum ImageCategory
    {
        Games,
        Avatars,
        General
    }

    public static class ImageService
    {
        public static readonly string BaseDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "RenPyTRLauncher", "Media");

        public static string GamesDir => Path.Combine(BaseDir, "games");
        public static string AvatarsDir => Path.Combine(BaseDir, "avatars");
        public static string GeneralDir => Path.Combine(BaseDir, "general");

        static ImageService()
        {
            Directory.CreateDirectory(GamesDir);
            Directory.CreateDirectory(AvatarsDir);
            Directory.CreateDirectory(GeneralDir);
        }

        public static string UploadFromFile(string sourcePath, ImageCategory category)
        {
            if (!File.Exists(sourcePath))
                throw new FileNotFoundException("Kaynak dosya bulunamadı.", sourcePath);

            var ext = Path.GetExtension(sourcePath).ToLowerInvariant();
            if (ext is not (".png" or ".jpg" or ".jpeg" or ".webp" or ".bmp" or ".gif"))
                ext = ".png";

            var folder = category switch
            {
                ImageCategory.Games => GamesDir,
                ImageCategory.Avatars => AvatarsDir,
                _ => GeneralDir
            };

            var fileName = $"{Guid.NewGuid():N}{ext}";
            var destPath = Path.Combine(folder, fileName);
            File.Copy(sourcePath, destPath, overwrite: true);

            var prefix = category switch
            {
                ImageCategory.Games => "media://games/",
                ImageCategory.Avatars => "media://avatars/",
                _ => "media://general/"
            };
            return prefix + fileName;
        }

        public static string SaveFromUrl(string url)
        {
            if (url.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                url.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                return url;
            return url;
        }

        public static string ResolvePath(string? storedPath)
        {
            if (string.IsNullOrWhiteSpace(storedPath)) return string.Empty;

            if (storedPath.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                storedPath.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                return storedPath;

            if (storedPath.StartsWith("media://", StringComparison.OrdinalIgnoreCase))
            {
                var relative = storedPath["media://".Length..];
                var parts = relative.Split('/', 2);
                if (parts.Length == 2)
                {
                    var folder = parts[0] switch
                    {
                        "games" => GamesDir,
                        "avatars" => AvatarsDir,
                        _ => GeneralDir
                    };
                    return Path.Combine(folder, parts[1]);
                }
            }

            if (storedPath.StartsWith("avatar://", StringComparison.OrdinalIgnoreCase))
                return storedPath;

            var full = Path.IsPathRooted(storedPath)
                ? storedPath
                : Path.Combine(AppContext.BaseDirectory, storedPath);
            return full;
        }

        public static string GetDefaultAvatar(string username)
        {
            var index = Math.Abs(username.GetHashCode()) % 8 + 1;
            return $"avatar://default_{index}";
        }

        public static bool IsDefaultAvatar(string? path) =>
            !string.IsNullOrWhiteSpace(path) && path.StartsWith("avatar://", StringComparison.OrdinalIgnoreCase);

        public static int GetDefaultAvatarIndex(string? path)
        {
            if (!IsDefaultAvatar(path)) return 1;
            var name = path!["avatar://".Length..];
            if (name.StartsWith("default_") && int.TryParse(name["default_".Length..], out var idx))
                return idx;
            return 1;
        }

        public static string[] GetAvailableCategories()
        {
            var names = ServiceLocator.CategoryService?.GetCategoryNames();
            if (names != null && names.Length > 0) return names;
            return new[] { "Devam Eden", "Biten", "Devam Etmeyen", "Erkek Başrol", "Kadın Başrol", "VIP", "Romance" };
        }
    }
}
