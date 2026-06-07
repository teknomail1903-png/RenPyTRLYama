using System;
using System.Windows;
using RenPyTRLauncher.Services;

namespace RenPyTRLauncher.Views
{
    public partial class ChangePasswordWindow : Window
    {
        private readonly Guid _userId;
        private readonly IAuthService _auth;

        public ChangePasswordWindow(Guid userId)
        {
            InitializeComponent();
            _userId = userId;
            _auth = ServiceLocator.AuthService ?? new AuthService(
                ServiceLocator.UserService ?? new InMemoryUserService());
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e) => Close();

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            if (TxtNew.Password != TxtNew2.Password)
            {
                MessageBox.Show("Yeni şifreler eşleşmiyor.", "Hata", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var result = _auth.ChangePassword(_userId, TxtCurrent.Password, TxtNew.Password);
            MessageBox.Show(result.Message, result.Success ? "Başarılı" : "Hata",
                MessageBoxButton.OK, result.Success ? MessageBoxImage.Information : MessageBoxImage.Warning);
            if (result.Success) Close();
        }
    }
}
