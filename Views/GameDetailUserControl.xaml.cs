using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using RenPyTRLauncher.Models;
using RenPyTRLauncher.Services;
using RenPyTRLauncher.ViewModels;

namespace RenPyTRLauncher.Views
{
    public partial class GameDetailUserControl : UserControl
    {
        private readonly Game _game;
        private readonly MainViewModel _viewModel;
        private bool _isFavorite;

        public event Action? BackRequested;

        public GameDetailUserControl(Game game, MainViewModel viewModel)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("[GameDetailUserControl] Constructor started");
                InitializeComponent();
                System.Diagnostics.Debug.WriteLine("[GameDetailUserControl] InitializeComponent completed");
                _game = game;
                _viewModel = viewModel;
                System.Diagnostics.Debug.WriteLine("[GameDetailUserControl] Loading game data");
                LoadGameData();
                System.Diagnostics.Debug.WriteLine("[GameDetailUserControl] Constructor completed successfully");
            }
            catch (System.Windows.Markup.XamlParseException ex)
            {
                System.Diagnostics.Debug.WriteLine("[GameDetailUserControl] XamlParseException: " + ex.Message);
                if (ex.InnerException != null)
                {
                    System.Diagnostics.Debug.WriteLine("[GameDetailUserControl] InnerException: " + ex.InnerException.Message);
                    System.Diagnostics.Debug.WriteLine("[GameDetailUserControl] InnerException Type: " + ex.InnerException.GetType().Name);
                }
                System.Diagnostics.Debug.WriteLine("[GameDetailUserControl] LineNumber: " + ex.LineNumber);
                System.Diagnostics.Debug.WriteLine("[GameDetailUserControl] LinePosition: " + ex.LinePosition);
                throw;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("[GameDetailUserControl] Exception: " + ex.Message);
                System.Diagnostics.Debug.WriteLine("[GameDetailUserControl] Exception Type: " + ex.GetType().Name);
                if (ex.InnerException != null)
                {
                    System.Diagnostics.Debug.WriteLine("[GameDetailUserControl] InnerException: " + ex.InnerException.Message);
                }
                throw;
            }
        }

        private void LoadGameData()
        {
            TxtName.Text = _game.Name;
            TxtVersion.Text = $"Oyun v{_game.Version}";
            TxtPatchVersion.Text = $"Yama {_game.PatchVersion}";
            TxtDescription.Text = string.IsNullOrWhiteSpace(_game.Description)
                ? "Bu oyun için henüz açıklama eklenmemiş."
                : _game.Description;
            TxtDescriptionFull.Text = string.IsNullOrWhiteSpace(_game.Description)
                ? "Bu oyun için henüz açıklama eklenmemiş."
                : _game.Description;
            TxtPatchNotes.Text = string.IsNullOrWhiteSpace(_game.PatchNotes)
                ? "Yama notları henüz eklenmemiş."
                : _game.PatchNotes;
            TxtDownloadCountCard.Text = _game.DownloadCount.ToString();
            TxtUpdatedDateCard.Text = _game.UpdatedDate.ToLocalTime().ToString("dd.MM.yyyy");

            try
            {
                var path = ImageService.ResolvePath(_game.ImagePath);
                if (!string.IsNullOrEmpty(path))
                {
                    var img = new System.Windows.Media.Imaging.BitmapImage();
                    img.BeginInit();
                    img.CacheOption = System.Windows.Media.Imaging.BitmapCacheOption.OnLoad;
                    img.UriSource = new Uri(path, UriKind.RelativeOrAbsolute);
                    img.EndInit();
                    img.Freeze();
                    ImgCover.Source = img;
                }
            }
            catch
            {
                ImgCover.Source = null;
            }

            BadgeVip.Visibility = _game.IsVip ? Visibility.Visible : Visibility.Collapsed;

            // Turkish Status Badge
            SetTurkishBadge();

            // Steam Status Badge
            SetSteamBadge();

            // Update Status Badge
            SetUpdateBadge();

            if (_game.ScreenshotPaths.Count > 0)
            {
                ScreenshotList.ItemsSource = _game.ScreenshotPaths;
                TxtNoScreenshots.Visibility = Visibility.Collapsed;
            }
            else
            {
                TxtNoScreenshots.Visibility = Visibility.Visible;
            }

            var links = ParseDownloadLinks(_game.DownloadLinks);
            if (links.Count > 0)
            {
                DownloadLinksList.ItemsSource = links;
                TxtNoLinks.Visibility = Visibility.Collapsed;
            }
            else
            {
                TxtNoLinks.Visibility = Visibility.Visible;
            }

            CategoryTags.ItemsSource = _game.Categories;

            // Load tags
            if (_game.Tags != null && _game.Tags.Count > 0)
            {
                TagList.ItemsSource = _game.Tags;
            }

            // Populate new info cards
            TxtPrimaryCategory.Text = _game.Categories.Count > 0 ? _game.Categories[0] : "Belirtilmemiş";
            TxtGameType.Text = _game.Type.ToString();
            TxtTurkishStatus.Text = _game.TurkishStatus.ToString();

            // Load new fields
            TxtDeveloper.Text = !string.IsNullOrEmpty(_game.Developer) ? _game.Developer : "Belirtilmemiş";
            TxtPublisher.Text = !string.IsNullOrEmpty(_game.Publisher) ? _game.Publisher : "Belirtilmemiş";
            TxtGameEngine.Text = !string.IsNullOrEmpty(_game.GameEngine) ? _game.GameEngine : "Belirtilmemiş";
            TxtReleaseDate.Text = _game.ReleaseDate.HasValue ? _game.ReleaseDate.Value.ToString("dd.MM.yyyy") : "Belirtilmemiş";
            TxtCompletionStatus.Text = _game.CompletionStatus.ToString();
            TxtAveragePlaytime.Text = !string.IsNullOrEmpty(_game.AveragePlaytime) ? _game.AveragePlaytime : "Belirtilmemiş";

            // Load Game Genres
            if (_game.GameGenres != null && _game.GameGenres.Count > 0)
            {
                GameGenresList.ItemsSource = _game.GameGenres;
            }
            else
            {
                GameGenresList.ItemsSource = new List<string> { "Belirtilmemiş" };
            }

            // Load Platforms
            if (_game.Platforms != null && _game.Platforms.Count > 0)
            {
                PlatformsList.ItemsSource = _game.Platforms;
            }
            else
            {
                PlatformsList.ItemsSource = new List<string> { "Belirtilmemiş" };
            }

            // Load Content Warnings
            if (_game.ContentWarnings != null && _game.ContentWarnings.Count > 0)
            {
                ContentWarningsList.ItemsSource = _game.ContentWarnings;
            }
            else
            {
                ContentWarningsList.ItemsSource = new List<string> { "Belirtilmemiş" };
            }

            RefreshFavoriteButton();
        }

        private void SetTurkishBadge()
        {
            BadgeTurkish.Visibility = Visibility.Visible;
            switch (_game.TurkishStatus)
            {
                case TurkishStatus.Evet:
                    BadgeTurkish.Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(39, 174, 96));
                    TxtTurkish.Text = "🇹🇷 Türkçe";
                    break;
                case TurkishStatus.Hayır:
                    BadgeTurkish.Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(52, 152, 219));
                    TxtTurkish.Text = "🌍 İngilizce";
                    break;
                case TurkishStatus.DevamEdiyor:
                    BadgeTurkish.Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(243, 156, 18));
                    TxtTurkish.Text = "🚧 Çeviri Devam Ediyor";
                    break;
                case TurkishStatus.KismiCeviri:
                    BadgeTurkish.Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(230, 126, 34));
                    TxtTurkish.Text = "� Kısmi Çeviri";
                    break;
            }
        }

        private void SetSteamBadge()
        {
            BadgeSteam.Visibility = Visibility.Visible;
            switch (_game.SteamStatus)
            {
                case SteamStatus.Var:
                    BadgeSteam.Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(30, 144, 255));
                    TxtSteam.Text = "🟦 Steam'de Var";
                    break;
                case SteamStatus.Yok:
                    BadgeSteam.Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(127, 140, 141));
                    TxtSteam.Text = "⚫ Steam'de Yok";
                    break;
            }
        }

        private void SetUpdateBadge()
        {
            BadgeUpdate.Visibility = Visibility.Visible;
            switch (_game.UpdateStatus)
            {
                case UpdateStatus.Updated:
                    BadgeUpdate.Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(39, 174, 96));
                    TxtUpdate.Text = "✅ Güncel";
                    break;
                case UpdateStatus.UpdateAvailable:
                    BadgeUpdate.Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(231, 76, 60));
                    TxtUpdate.Text = "🔄 Güncelleme Var";
                    break;
                case UpdateStatus.Outdated:
                    BadgeUpdate.Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(149, 165, 166));
                    TxtUpdate.Text = "⚠️ Eski";
                    break;
            }
        }

        private void RefreshFavoriteButton()
        {
            _isFavorite = _viewModel.IsFavorite(_game.Id);
            BtnFavorite.Content = _isFavorite ? "★ Favorilerden Çıkar" : "☆ Favorilere Ekle";
        }

        private static List<DownloadLinkItem> ParseDownloadLinks(List<string> links)
        {
            var result = new List<DownloadLinkItem>();
            foreach (var link in links)
            {
                var parts = link.Split('|', 2);
                if (parts.Length == 2)
                    result.Add(new DownloadLinkItem { Label = parts[0].Trim(), Url = parts[1].Trim() });
                else if (link.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                    result.Add(new DownloadLinkItem { Label = "Bağlantı", Url = link.Trim() });
            }
            return result;
        }

        private void BtnFavorite_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.ToggleFavorite(_game);
            RefreshFavoriteButton();
        }

        private async void BtnInstallPatch_Click(object sender, RoutedEventArgs e)
        {
            if (_viewModel.CurrentUser == null)
            {
                MessageBox.Show("Kullanıcı oturumu bulunamadı.", "Yama Kur", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!AuthorizationService.CanInstallVipPatch(_viewModel.CurrentUser, _game))
            {
                MessageBox.Show("Bu yama VIP üyelere özeldir.", "VIP Gerekli", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var dialog = new OpenFolderDialog
            {
                Title = $"{_game.Name} — Oyun klasörünü seçin (game/ klasörünün üst dizini)"
            };
            if (dialog.ShowDialog() != true) return;

            BtnInstallPatch.IsEnabled = false;
            try
            {
                var result = await _viewModel.InstallPatchAsync(_game, dialog.FolderName);
                var msg = result.Message;
                if (result.BackupPath != null) msg += $"\n\nYedek: {result.BackupPath}";
                MessageBox.Show(msg, result.Success ? "Kurulum Başarılı" : "Kurulum Başarısız",
                    MessageBoxButton.OK, result.Success ? MessageBoxImage.Information : MessageBoxImage.Error);
            }
            finally
            {
                BtnInstallPatch.IsEnabled = true;
            }
        }

        private async void BtnDownloadGame_Click(object sender, RoutedEventArgs e)
        {
            var downloadService = ServiceLocator.DownloadService ?? new DownloadService();
            
            var saveDialog = new SaveFileDialog
            {
                FileName = $"{_game.Name}.zip",
                Filter = "ZIP Dosyaları|*.zip|Tüm Dosyalar|*.*",
                Title = $"{_game.Name} - İndirme Konumu Seçin"
            };
            
            if (saveDialog.ShowDialog() != true) return;
            
            var progressWindow = new DownloadProgressWindow();
            var cts = new CancellationTokenSource();
            progressWindow.SetCancellationTokenSource(cts);
            progressWindow.Show();
            
            try
            {
                var progress = new Progress<DownloadProgress>(p => progressWindow.SetProgress(p));
                var result = await downloadService.DownloadGameAsync(_game, saveDialog.FileName, progress, cts.Token);
                
                if (result.Success)
                {
                    MessageBox.Show(result.Message, "İndirme Tamamlandı", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show(result.Message, "İndirme Başarısız", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"İndirme hatası: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                if (progressWindow.IsVisible)
                    progressWindow.Close();
                cts.Dispose();
            }
        }

        private void DownloadLink_Click(object sender, RoutedEventArgs e)
        {
            if (sender is System.Windows.Controls.Button btn && btn.Tag is string url)
            {
                try
                {
                    Process.Start(new ProcessStartInfo { FileName = url, UseShellExecute = true });
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Link açılamadı: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
        }

        private void Screenshot_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (sender is System.Windows.Controls.Border border && border.DataContext is string imagePath)
            {
                try
                {
                    var resolvedPath = ImageService.ResolvePath(imagePath);
                    if (!string.IsNullOrEmpty(resolvedPath))
                    {
                        var viewer = new ImageViewerWindow(resolvedPath);
                        viewer.ShowDialog();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Görüntü açılamadı: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void Tag_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (sender is System.Windows.Controls.Border border && border.DataContext is string tag)
            {
                // Trigger back navigation with tag filter
                BackRequested?.Invoke();
                // Note: Tag filtering would be implemented in MainWindow
                // For now, we'll just go back
            }
        }

        private class DownloadLinkItem
        {
            public string Label { get; set; } = "";
            public string Url { get; set; } = "";
        }
    }
}
