using System.Windows;
using System.Windows.Input;
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

            // Enable window dragging
            MouseLeftButtonDown += (s, e) => DragMove();

            // Set shutdown mode to ensure single-click close
            App.Current.ShutdownMode = ShutdownMode.OnExplicitShutdown;
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

        private void BtnForgotPassword_Click(object sender, RoutedEventArgs e)
        {
            // Open forgot password window (placeholder)
            var forgotPasswordWindow = new ForgotPasswordWindow();
            forgotPasswordWindow.ShowDialog();
        }

        private void BtnMinimize_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void BtnMaximize_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            App.Current.Shutdown();
        }

        private void ShowError(string msg)
        {
            TxtError.Text = msg;
            TxtError.Visibility = Visibility.Visible;
        }

        private void TxtLoginUser_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                TxtLoginPass.Focus();
                e.Handled = true;
            }
        }

        private void TxtLoginPass_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                BtnLogin_Click(sender, e);
                e.Handled = true;
            }
        }

        private void OpenMain()
        {
            var main = new MainWindow();
            main.Show();
            Close();
            App.Current.ShutdownMode = ShutdownMode.OnLastWindowClose;
        }
    }
}
