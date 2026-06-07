using System;
using System.Globalization;
using System.IO;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using RenPyTRLauncher.Services;

namespace RenPyTRLauncher.Data
{
    public class SafeImageConverter : IValueConverter
    {
        public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var path = value as string;
            if (string.IsNullOrWhiteSpace(path)) return null;
            if (ImageService.IsDefaultAvatar(path)) return null;

            try
            {
                var full = ImageService.ResolvePath(path);
                if (string.IsNullOrWhiteSpace(full)) return null;

                if (full.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                    full.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                {
                    return new BitmapImage(new Uri(full, UriKind.Absolute));
                }

                if (!File.Exists(full)) return null;

                var img = new BitmapImage();
                img.BeginInit();
                img.CacheOption = BitmapCacheOption.OnLoad;
                img.UriSource = new Uri(full, UriKind.Absolute);
                img.EndInit();
                img.Freeze();
                return img;
            }
            catch
            {
                return null;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
            throw new NotSupportedException();
    }
}
