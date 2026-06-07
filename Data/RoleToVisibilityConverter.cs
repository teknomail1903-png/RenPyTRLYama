using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using RenPyTRLauncher.Models;
using RenPyTRLauncher.Services;

namespace RenPyTRLauncher.Data
{
    public class RoleToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var user = value as User;
            var requiredRole = parameter?.ToString() ?? UserRole.Admin;
            var visible = requiredRole switch
            {
                "Mod" => AuthorizationService.CanAccessAdminPanel(user),
                "Admin" => AuthorizationService.CanManageGames(user),
                "Vip" => user?.IsVip == true,
                _ => false
            };
            return visible ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
            throw new NotSupportedException();
    }
}
