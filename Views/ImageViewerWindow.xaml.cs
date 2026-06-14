using System.Windows;

namespace RenPyTRLauncher.Views
{
    public partial class ImageViewerWindow : Window
    {
        public ImageViewerWindow(string imagePath)
        {
            InitializeComponent();
            LoadImage(imagePath);
        }

        private void LoadImage(string imagePath)
        {
            try
            {
                ImgViewer.Source = new System.Windows.Media.Imaging.BitmapImage(new Uri(imagePath, UriKind.RelativeOrAbsolute));
            }
            catch
            {
                MessageBox.Show("Görüntü yüklenemedi.", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
                Close();
            }
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
