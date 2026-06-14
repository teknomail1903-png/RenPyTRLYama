using System;
using System.Windows;
using RenPyTRLauncher.Services;
using RenPyTRLauncher.Models;

namespace RenPyTRLauncher.Views
{
    public partial class ForgotPasswordWindow : Window
    {
        private readonly IAuthService _auth;
        private readonly IUserService _userService;
        private User? _currentUser;
        private int _currentStep = 1;

        public ForgotPasswordWindow()
        {
            InitializeComponent();
            
            _auth = ServiceLocator.AuthService ?? new AuthService(
                ServiceLocator.UserService ?? new InMemoryUserService());
            _userService = ServiceLocator.UserService ?? new InMemoryUserService();

            MouseLeftButtonDown += (s, e) => DragMove();
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void BtnStep1Continue_Click(object sender, RoutedEventArgs e)
        {
            var email = TxtEmail.Text?.Trim();
            
            if (string.IsNullOrEmpty(email))
            {
                MessageBox.Show("E-posta adresi boş olamaz.", "Hata", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var user = _userService.GetByEmail(email);
            if (user == null)
            {
                MessageBox.Show("Bu e-posta adresiyle kayıtlı kullanıcı bulunamadı.", "Hata", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrEmpty(user.SecretQuestion) || string.IsNullOrEmpty(user.SecretAnswer))
            {
                MessageBox.Show("Bu hesap için gizli soru ayarlanmamış. Lütfen yönetici ile iletişime geçin.", "Hata", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            _currentUser = user;
            TxtSecretQuestionDisplay.Text = user.SecretQuestion;
            
            Step1Panel.Visibility = Visibility.Collapsed;
            Step2Panel.Visibility = Visibility.Visible;
            _currentStep = 2;
        }

        private void BtnStep2Back_Click(object sender, RoutedEventArgs e)
        {
            Step2Panel.Visibility = Visibility.Collapsed;
            Step1Panel.Visibility = Visibility.Visible;
            _currentStep = 1;
        }

        private void BtnStep2Continue_Click(object sender, RoutedEventArgs e)
        {
            var answer = TxtSecretAnswer.Text?.Trim();
            
            if (string.IsNullOrEmpty(answer))
            {
                MessageBox.Show("Gizli cevap boş olamaz.", "Hata", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (_currentUser == null)
            {
                MessageBox.Show("Kullanıcı bilgisi bulunamadı.", "Hata", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!string.Equals(answer, _currentUser.SecretAnswer, StringComparison.OrdinalIgnoreCase))
            {
                MessageBox.Show("Gizli cevap yanlış.", "Hata", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            Step2Panel.Visibility = Visibility.Collapsed;
            Step3Panel.Visibility = Visibility.Visible;
            _currentStep = 3;
        }

        private void BtnStep3Back_Click(object sender, RoutedEventArgs e)
        {
            Step3Panel.Visibility = Visibility.Collapsed;
            Step2Panel.Visibility = Visibility.Visible;
            _currentStep = 2;
        }

        private void BtnStep3Continue_Click(object sender, RoutedEventArgs e)
        {
            var newPassword = TxtNewPassword.Password;
            var newPasswordConfirm = TxtNewPasswordConfirm.Password;
            
            if (string.IsNullOrEmpty(newPassword))
            {
                MessageBox.Show("Yeni şifre boş olamaz.", "Hata", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (newPassword.Length < 6)
            {
                MessageBox.Show("Şifre minimum 6 karakter olmalıdır.", "Hata", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (newPassword != newPasswordConfirm)
            {
                MessageBox.Show("Şifreler eşleşmiyor.", "Hata", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (_currentUser == null)
            {
                MessageBox.Show("Kullanıcı bilgisi bulunamadı.", "Hata", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var result = _auth.ResetPassword(_currentUser.Id, newPassword);
            
            if (!result.Success)
            {
                MessageBox.Show(result.Message, "Hata", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            Step3Panel.Visibility = Visibility.Collapsed;
            Step4Panel.Visibility = Visibility.Visible;
            _currentStep = 4;
        }

        private void BtnStep4Finish_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
