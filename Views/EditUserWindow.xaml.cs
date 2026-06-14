using System.Linq;
using System.Windows;
using RenPyTRLauncher.Models;
using RenPyTRLauncher.Services;

namespace RenPyTRLauncher.Views
{
    public partial class EditUserWindow : Window
    {
        private readonly IUserService _userService;
        private readonly User _user;
        private readonly bool _isNew;

        public EditUserWindow(User? user, IUserService userService)
        {
            InitializeComponent();
            _userService = userService;
            if (user == null)
            {
                _user = new User { Role = UserRole.User, MembershipLevel = "Ücretsiz" };
                _isNew = true;
            }
            else
            {
                _user = user;
                _isNew = false;
            }

            CmbRole.ItemsSource = new[] { UserRole.User, UserRole.Mod, UserRole.Admin };
            CmbMembership.ItemsSource = new[] { "Ücretsiz", "Bronz", "Gümüş", "Altın", "Platin", "Elmas", "VIP" };
            CmbSecretQuestion.ItemsSource = new[]
            {
                "İlk evcil hayvanınızın adı nedir?",
                "Annenizin kızlık soyadı nedir?",
                "İlk okulunuzun adı nedir?",
                "Doğduğunuz şehir nedir?",
                "En sevdiğiniz yemek nedir?",
                "İlk arabanızın markası nedir?",
                "En sevdiğiniz film nedir?",
                "Çocukluk kahramanınız kimdi?"
            };
            TxtUsername.Text = _user.Username;
            TxtEmail.Text = _user.Email;
            CmbRole.SelectedItem = _user.Role;
            CmbMembership.SelectedItem = _user.MembershipLevel;
            TxtAvatar.Text = string.IsNullOrEmpty(_user.AvatarPath)
                ? ImageService.GetDefaultAvatar(_user.Username)
                : _user.AvatarPath;
            
            if (!_isNew && !string.IsNullOrEmpty(_user.SecretQuestion))
            {
                CmbSecretQuestion.SelectedItem = _user.SecretQuestion;
                TxtSecretAnswer.Text = _user.SecretAnswer;
            }
        }

        private void BtnBrowseAvatar_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "Resim|*.png;*.jpg;*.jpeg;*.webp;*.bmp|Tümü|*.*"
            };
            if (dlg.ShowDialog() != true) return;
            try
            {
                TxtAvatar.Text = ImageService.UploadFromFile(dlg.FileName, ImageCategory.Avatars);
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(ex.Message, "Yükleme Hatası", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e) => Close();

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            var username = TxtUsername.Text.Trim();
            var email = TxtEmail.Text.Trim();
            var password = TxtPassword.Password;
            var passwordConfirm = TxtPasswordConfirm.Password;
            var secretQuestion = CmbSecretQuestion.SelectedItem?.ToString() ?? string.Empty;
            var secretAnswer = TxtSecretAnswer.Text?.Trim() ?? string.Empty;

            if (username.Length < 3)
            {
                MessageBox.Show("Kullanıcı adı en az 3 karakter olmalı.", "Hata", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (_isNew)
            {
                if (_userService.GetByUsername(username) != null)
                {
                    MessageBox.Show("Bu kullanıcı adı kullanılıyor.", "Hata", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (string.IsNullOrEmpty(password))
                {
                    MessageBox.Show("Şifre alanı zorunludur.", "Hata", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (password.Length < 6)
                {
                    MessageBox.Show("Şifre en az 6 karakter olmalı.", "Hata", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (password != passwordConfirm)
                {
                    MessageBox.Show("Şifreler eşleşmiyor.", "Hata", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                _user.Username = username;
                _user.Email = email;
                _user.PasswordHash = PasswordHasher.Hash(password);
                _user.Role = CmbRole.SelectedItem?.ToString() ?? UserRole.User;
                _user.MembershipLevel = CmbMembership.SelectedItem?.ToString() ?? "Ücretsiz";
                _user.AvatarPath = TxtAvatar.Text;
                _user.SecretQuestion = secretQuestion;
                _user.SecretAnswer = secretAnswer;
                AuthorizationService.SyncVipBadges(_user);
                _userService.Create(_user);
            }
            else
            {
                _user.Username = username;
                _user.Email = email;
                _user.Role = CmbRole.SelectedItem?.ToString() ?? UserRole.User;
                _user.MembershipLevel = CmbMembership.SelectedItem?.ToString() ?? "Ücretsiz";
                _user.AvatarPath = TxtAvatar.Text;

                // Only update password if provided
                if (!string.IsNullOrEmpty(password))
                {
                    if (password.Length < 6)
                    {
                        MessageBox.Show("Şifre en az 6 karakter olmalı.", "Hata", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    if (password != passwordConfirm)
                    {
                        MessageBox.Show("Şifreler eşleşmiyor.", "Hata", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    _user.PasswordHash = PasswordHasher.Hash(password);
                }

                // Only update secret question/answer if provided
                if (!string.IsNullOrEmpty(secretQuestion) && !string.IsNullOrEmpty(secretAnswer))
                {
                    _user.SecretQuestion = secretQuestion;
                    _user.SecretAnswer = secretAnswer;
                }

                AuthorizationService.SyncVipBadges(_user);
                _userService.Update(_user);
            }

            ServiceLocator.NotifyDataChanged();
            Close();
        }
    }
}
