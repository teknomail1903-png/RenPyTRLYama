using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using RenPyTRLauncher.Services;

namespace RenPyTRLauncher.Data
{
    public class AvatarImageConverter : IValueConverter
    {
        private static readonly Color[] AvatarColors =
        {
            Color.FromRgb(155, 89, 255),
            Color.FromRgb(52, 152, 219),
            Color.FromRgb(46, 204, 113),
            Color.FromRgb(241, 196, 15),
            Color.FromRgb(231, 76, 60),
            Color.FromRgb(230, 126, 34),
            Color.FromRgb(142, 68, 173),
            Color.FromRgb(26, 188, 156)
        };

        public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var path = value as string;
            if (ImageService.IsDefaultAvatar(path))
            {
                var idx = ImageService.GetDefaultAvatarIndex(path) - 1;
                if (idx < 0 || idx >= AvatarColors.Length) idx = 0;
                return new SolidColorBrush(AvatarColors[idx]);
            }

            if (string.IsNullOrWhiteSpace(path)) return new SolidColorBrush(Color.FromRgb(42, 42, 53));

            var imgConverter = new SafeImageConverter();
            return imgConverter.Convert(path!, targetType, parameter, culture)
                   ?? new SolidColorBrush(Color.FromRgb(42, 42, 53));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
            throw new NotSupportedException();
    }
}
