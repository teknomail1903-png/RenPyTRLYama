using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace RenPyTRLauncher.Data
{
    public class StringToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string s && !string.IsNullOrWhiteSpace(s))
            {
                try { return new BrushConverter().ConvertFromString(s) as Brush ?? Brushes.Transparent; }
                catch { }
            }
            return new SolidColorBrush(Color.FromRgb(155, 89, 255));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
            throw new NotSupportedException();
    }
}
