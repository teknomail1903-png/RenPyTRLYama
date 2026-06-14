using System;
using System.Windows;
using RenPyTRLauncher.Services;
using RenPyTRLauncher.Models;

namespace RenPyTRLauncher.Views
{
    public partial class RegisterWindow : Window
    {
        private readonly IAuthService _auth;

        public RegisterWindow()
        {
            InitializeComponent();
            
            _auth = ServiceLocator.AuthService;

            MouseLeftButtonDown += (s, e) => DragMove();

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
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void BtnRegister_Click(object sender, RoutedEventArgs e)
        {
            var username = TxtUsername.Text?.Trim();
            var email = TxtEmail.Text?.Trim();
            var password = TxtPassword.Password;
            var passwordConfirm = TxtPasswordConfirm.Password;
            var secretQuestion = CmbSecretQuestion.SelectedItem?.ToString() ?? string.Empty;
            var secretAnswer = TxtSecretAnswer.Text?.Trim() ?? string.Empty;

            if (string.IsNullOrEmpty(username))
            {
                MessageBox.Show("Kullanıcı adı boş olamaz.", "Hata", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (username.Length < 3)
            {
                MessageBox.Show("Kullanıcı adı en az 3 karakter olmalıdır.", "Hata", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrEmpty(email))
            {
                MessageBox.Show("E-posta adresi boş olamaz.", "Hata", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!email.Contains('@'))
            {
                MessageBox.Show("Geçerli bir e-posta adresi girin.", "Hata", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Şifre boş olamaz.", "Hata", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (password.Length < 6)
            {
                MessageBox.Show("Şifre en az 6 karakter olmalıdır.", "Hata", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (password != passwordConfirm)
            {
                MessageBox.Show("Şifreler eşleşmiyor.", "Hata", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrEmpty(secretQuestion))
            {
                MessageBox.Show("Gizli soru seçmelisiniz.", "Hata", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrEmpty(secretAnswer))
            {
                MessageBox.Show("Gizli cevap boş olamaz.", "Hata", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var result = _auth.Register(username, email, password, secretQuestion, secretAnswer);
            
            if (!result.Success)
            {
                MessageBox.Show(result.Message, "Kayıt Hatası", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            MessageBox.Show("Kayıt başarılı! Giriş yapabilirsiniz.", "Başarılı", MessageBoxButton.OK, MessageBoxImage.Information);
            Close();
        }
    }
}
