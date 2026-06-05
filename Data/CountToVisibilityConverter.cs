using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace RenPyTRLauncher.Data
{
    public class CountToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var count = value is int i ? i : System.Convert.ToInt32(value ?? 0);
            var mode = parameter?.ToString();
            var visible = mode switch
            {
                "Empty" => count == 0,
                "NotEmpty" => count > 0,
                _ => count > 0
            };
            return visible ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
            throw new NotSupportedException();
    }
}
