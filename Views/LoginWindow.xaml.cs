using System.Windows;
using RenPyTRLauncher.Services;

namespace RenPyTRLauncher.Views
{
    public partial class LoginWindow : Window
    {
        private readonly IAuthService _auth;

        public LoginWindow()
        {
            InitializeComponent();
            _auth = ServiceLocator.AuthService ?? new AuthService(
                ServiceLocator.UserService ?? new InMemoryUserService());
        }

        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            TxtError.Visibility = Visibility.Collapsed;
            var result = _auth.Login(TxtLoginUser.Text, TxtLoginPass.Password, ChkRemember.IsChecked == true);
            if (!result.Success)
            {
                ShowError(result.Message);
                return;
            }
            OpenMain();
        }

        private void BtnRegister_Click(object sender, RoutedEventArgs e)
        {
            TxtError.Visibility = Visibility.Collapsed;
            if (TxtRegPass.Password != TxtRegPass2.Password)
            {
                ShowError("Şifreler eşleşmiyor.");
                return;
            }

            var result = _auth.Register(TxtRegUser.Text, TxtRegEmail.Text, TxtRegPass.Password);
            if (!result.Success)
            {
                ShowError(result.Message);
                return;
            }

            SessionService.Save(result.User!.Id, false);
            OpenMain();
        }

        private void ShowError(string msg)
        {
            TxtError.Text = msg;
            TxtError.Visibility = Visibility.Visible;
        }

        private void OpenMain()
        {
            var main = new MainWindow();
            main.Show();
            Close();
        }
    }
}
