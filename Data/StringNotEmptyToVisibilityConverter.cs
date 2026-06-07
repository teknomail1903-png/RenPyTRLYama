using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace RenPyTRLauncher.Data
{
    public class StringNotEmptyToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var text = value as string;
            var visible = !string.IsNullOrWhiteSpace(text);
            if (parameter?.ToString() == "Invert") visible = !visible;
            return visible ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
            throw new NotSupportedException();
    }
}
