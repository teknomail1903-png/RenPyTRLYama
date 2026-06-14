using System;
using System.Threading;
using System.Windows;
using RenPyTRLauncher.Models;

namespace RenPyTRLauncher.Views
{
    public partial class DownloadProgressWindow : Window
    {
        private CancellationTokenSource? _cts;

        public DownloadProgressWindow()
        {
            InitializeComponent();
        }

        public void SetProgress(DownloadProgress progress)
        {
            Dispatcher.Invoke(() =>
            {
                TxtGameName.Text = $"{progress.GameName} indiriliyor...";
                ProgressBar.Value = progress.Percentage;
                TxtPercentage.Text = $"{progress.Percentage:F1}%";

                if (progress.Status == DownloadStatus.Completed)
                {
                    BtnCancel.IsEnabled = false;
                    BtnCancel.Content = "Tamamlandı";
                    Close();
                }
                else if (progress.Status == DownloadStatus.Failed)
                {
                    BtnCancel.IsEnabled = false;
                    BtnCancel.Content = "Başarısız";
                    MessageBox.Show($"İndirme başarısız: {progress.ErrorMessage}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
                    Close();
                }
                else if (progress.Status == DownloadStatus.Cancelled)
                {
                    BtnCancel.IsEnabled = false;
                    BtnCancel.Content = "İptal Edildi";
                    Close();
                }
            });
        }

        public void SetCancellationTokenSource(CancellationTokenSource cts)
        {
            _cts = cts;
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            _cts?.Cancel();
            BtnCancel.IsEnabled = false;
            BtnCancel.Content = "İptal Ediliyor...";
        }
    }
}
