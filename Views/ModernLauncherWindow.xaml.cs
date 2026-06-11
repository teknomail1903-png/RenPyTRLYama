using System.Windows;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using RenPyTRLauncher.Services;

namespace RenPyTRLauncher.Views
{
    public partial class ModernLauncherWindow : Window
    {
        private readonly IAuthService? _auth;

        public ModernLauncherWindow()
        {
            InitializeComponent();
            
            try
            {
                _auth = ServiceLocator.AuthService ?? new AuthService(
                    ServiceLocator.UserService ?? new InMemoryUserService());
            }
            catch
            {
                // Ignore errors for now
            }

            // Enable window dragging
            MouseLeftButtonDown += (s, e) => DragMove();
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

        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            App.Log("[LOGIN] Button clicked");

            try
            {
                App.Log("[LOGIN] Checking _auth null status");
                if (_auth == null)
                {
                    App.Log("[LOGIN] ERROR: _auth is NULL");
                    MessageBox.Show("Authentication service is not initialized.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                App.Log("[LOGIN] AuthService NULL: False");

                App.Log("[LOGIN] Checking controls null status");
                if (TxtUsername == null || TxtPassword == null)
                {
                    App.Log("[LOGIN] ERROR: TxtUsername or TxtPassword is NULL");
                    MessageBox.Show("Login controls not found.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                App.Log("[LOGIN] Reading username");
                var username = TxtUsername.Text?.Trim();
                App.Log($"[LOGIN] Username: {username ?? "(null)"}");

                App.Log("[LOGIN] Reading password");
                var password = GetPassword(TxtPassword, TxtPasswordVisible);
                App.Log($"[LOGIN] Password length: {password?.Length ?? 0}");

                var remember = ChkRemember?.IsChecked == true;
                App.Log($"[LOGIN] Remember me: {remember}");

                if (string.IsNullOrEmpty(username))
                {
                    App.Log("[LOGIN] ERROR: Username is empty");
                    MessageBox.Show("Kullanıcı adı boş olamaz.", "Giriş Hatası", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (string.IsNullOrEmpty(password))
                {
                    App.Log("[LOGIN] ERROR: Password is empty");
                    MessageBox.Show("Şifre boş olamaz.", "Giriş Hatası", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                App.Log("[LOGIN] Calling Authenticate");
                var result = _auth.Login(username, password, remember);
                App.Log($"[LOGIN] Authenticate result - Success: {result.Success}, Message: {result.Message}");

                if (!result.Success)
                {
                    App.Log("[LOGIN] ERROR: Login failed");
                    MessageBox.Show(result.Message, "Giriş Hatası", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                App.Log("[LOGIN] Login successful, opening MainWindow");
                // Open main window
                var main = new MainWindow();
                main.Show();
                Close();
                App.Log("[LOGIN] MainWindow opened, login window closed");
            }
            catch (Exception ex)
            {
                App.Log($"[LOGIN] EXCEPTION: {ex.Message}\n{ex.StackTrace}");
                MessageBox.Show($"Login error: {ex.Message}\n\n{ex.StackTrace}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnRegister_Click(object sender, RoutedEventArgs e)
        {
            // Open registration window or switch to registration tab
            MessageBox.Show("Kayıt özelliği yakında eklenecek.", "Bilgi", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void BtnForgotPassword_Click(object sender, RoutedEventArgs e)
        {
            var forgotPasswordWindow = new ForgotPasswordWindow();
            forgotPasswordWindow.ShowDialog();
        }

        private void BtnTogglePassword_Click(object sender, RoutedEventArgs e)
        {
            TogglePasswordVisibility(TxtPassword, TxtPasswordVisible, BtnTogglePassword);
        }

        private void TogglePasswordVisibility(System.Windows.Controls.PasswordBox passwordBox, System.Windows.Controls.TextBox textBox, System.Windows.Controls.Button toggleButton)
        {
            if (passwordBox.Visibility == Visibility.Visible)
            {
                textBox.Text = passwordBox.Password;
                passwordBox.Visibility = Visibility.Collapsed;
                textBox.Visibility = Visibility.Visible;
                toggleButton.Content = "👁‍🗨";
            }
            else
            {
                passwordBox.Password = textBox.Text;
                textBox.Visibility = Visibility.Collapsed;
                passwordBox.Visibility = Visibility.Visible;
                toggleButton.Content = "👁";
            }
        }

        private string GetPassword(System.Windows.Controls.PasswordBox passwordBox, System.Windows.Controls.TextBox textBox)
        {
            return passwordBox.Visibility == Visibility.Visible ? passwordBox.Password : textBox.Text;
        }
    }
}
