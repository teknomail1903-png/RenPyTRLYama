using System.Windows;

namespace RenPyTRLauncher.Views
{
    public partial class ForgotPasswordWindow : Window
    {
        public ForgotPasswordWindow()
        {
            InitializeComponent();

            // Enable window dragging
            MouseLeftButtonDown += (s, e) => DragMove();
        }

        private void BtnSend_Click(object sender, RoutedEventArgs e)
        {
            // Placeholder for forgot password functionality
            // TODO: Implement forgot password flow with Firebase/API integration
            MessageBox.Show("Şifre sıfırlama özelliği yakında eklenecek.\n\nBu özellik Firebase entegrasyonu sonrasında aktif olacaktır.",
                "Şifremi Unuttum", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
