using System;
using System.Diagnostics;
using System.Windows;
using Microsoft.Win32;
using RenPyTRLauncher.Models;
using RenPyTRLauncher.ViewModels;

namespace RenPyTRLauncher
{
    public partial class MainWindow : Window
    {
        private readonly MainViewModel viewModel;
        private readonly System.Collections.Generic.Dictionary<string, System.Windows.Controls.Button> _sidebarMap = new();
        private string _activePage = "PageAnaSayfa";

        public MainWindow()
        {
            InitializeComponent();

            var db = Data.DatabaseInitializer.Initialize();

            Services.ServiceLocator.GameService = new Services.EfGameService(db);
            Services.ServiceLocator.AnnouncementService = new Services.EfAnnouncementService(db);
            Services.ServiceLocator.UserService = new Services.EfUserService(db);
            Services.ServiceLocator.SettingsService = new Services.EfSettingsService(db);
            Services.ServiceLocator.MembershipService = new Services.EfMembershipService(db);
            Services.ServiceLocator.ActivityService = new Services.EfActivityService(db);
            Services.ServiceLocator.PatchService = new Services.PatchService(
                Services.ServiceLocator.GameService,
                Services.ServiceLocator.UserService);

            viewModel = new MainViewModel();
            DataContext = viewModel;

            WireSidebarButtons();
            WireSettingsButtons();
            SetActivePage("PageAnaSayfa");
        }

        private void WireSidebarButtons()
        {
            _sidebarMap["PageAnaSayfa"] = BtnAnaSayfa;
            _sidebarMap["PageOyunlar"] = BtnOyunlar;
            _sidebarMap["PageKategori"] = BtnKategori;
            _sidebarMap["PageTop10"] = BtnTop10;
            _sidebarMap["PageVip"] = BtnVip;
            _sidebarMap["PageProfil"] = BtnProfil;
            _sidebarMap["PageAyarlar"] = BtnAyarlar;

            BtnAnaSayfa.Click += (_, _) => SetActivePage("PageAnaSayfa");
            BtnOyunlar.Click += (_, _) => SetActivePage("PageOyunlar");
            BtnKategori.Click += (_, _) => { viewModel.CloseCategoryFolder(); SetActivePage("PageKategori"); };
            BtnTop10.Click += (_, _) => SetActivePage("PageTop10");
            BtnVip.Click += (_, _) => SetActivePage("PageVip");
            BtnProfil.Click += (_, _) => SetActivePage("PageProfil");
            BtnAyarlar.Click += (_, _) => SetActivePage("PageAyarlar");
            BtnAdmin.Click += (_, _) => ShowAdmin();
            BtnLogout.Click += BtnLogout_Click;
        }

        private void WireSettingsButtons()
        {
            var settingsButtons = new (System.Windows.Controls.Button btn, Action action)[]
            {
                (BtnCheckUpdates, () => MessageBox.Show("Güncelleme kontrolü: En son sürüm kullanılıyor (v1.1).", "Güncellemeler", MessageBoxButton.OK, MessageBoxImage.Information)),
                (BtnClearSaves, () => ClearFolder("Saves", "Kayıt dosyaları")),
                (BtnClearCache, () => ClearFolder("Cache", "Önbellek")),
                (BtnClearLogs, () => ClearFolder("Logs", "Log dosyaları")),
                (BtnOpenGamesFolder, () => OpenAppFolder("Games")),
                (BtnOpenBackupFolder, () => OpenAppFolder("Backups"))
            };

            foreach (var (btn, action) in settingsButtons)
                btn.Click += (_, _) => action();

            BtnRollbackLast.Click += (_, _) =>
            {
                var latest = Services.RollbackService.GetLatestBackup();
                if (latest == null)
                {
                    MessageBox.Show("Geri alınacak yedek bulunamadı.", "Rollback", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }
                if (MessageBox.Show($"Son yedek geri yüklensin mi?\n{latest}", "Rollback",
                        MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes) return;

                var result = Services.RollbackService.RollbackFromFolder(latest);
                MessageBox.Show(result.Message, result.Success ? "Rollback Başarılı" : "Rollback Başarısız",
                    MessageBoxButton.OK, result.Success ? MessageBoxImage.Information : MessageBoxImage.Error);
            };
        }

        private void AnnPrev_Click(object sender, RoutedEventArgs e) => viewModel.PrevAnnouncement();
        private void AnnNext_Click(object sender, RoutedEventArgs e) => viewModel.NextAnnouncement();

        private void CategoryFolder_Click(object sender, RoutedEventArgs e)
        {
            if (sender is System.Windows.Controls.Button btn && btn.Tag is CategoryFolderItem folder)
                viewModel.OpenCategoryFolder(folder.CategoryKey, folder.DisplayName);
        }

        private void CategoryBack_Click(object sender, RoutedEventArgs e) => viewModel.CloseCategoryFolder();

        private void PurchaseMembership_Click(object sender, RoutedEventArgs e)
        {
            if (sender is System.Windows.Controls.Button btn && btn.Tag is string url)
                OpenUrl(url);
        }

        private void OpenLink_Click(object sender, RoutedEventArgs e)
        {
            if (sender is System.Windows.Controls.Button btn && btn.Tag is string url)
                OpenUrl(url);
        }

        private static void OpenUrl(string url)
        {
            if (string.IsNullOrWhiteSpace(url)) return;
            try
            {
                Process.Start(new ProcessStartInfo { FileName = url, UseShellExecute = true });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Link açılamadı: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private async void InstallPatch_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not System.Windows.Controls.Button btn) return;
            var game = btn.Tag as Game ?? (btn.Tag as LeaderboardEntry)?.Game;
            if (game == null) return;

            if (viewModel.CurrentUser == null)
            {
                MessageBox.Show("Kullanıcı oturumu bulunamadı.", "Yama Kur", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (game.IsVip && viewModel.CurrentUser.IsVip != true)
            {
                MessageBox.Show("Bu yama VIP üyelere özeldir.", "VIP Gerekli", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var dialog = new OpenFolderDialog
            {
                Title = $"{game.Name} — Oyun klasörünü seçin (game/ klasörünün üst dizini)"
            };

            if (dialog.ShowDialog() != true) return;

            btn.IsEnabled = false;
            try
            {
                var result = await viewModel.InstallPatchAsync(game, dialog.FolderName);
                var msg = result.Message;
                if (result.BackupPath != null) msg += $"\n\nYedek: {result.BackupPath}";
                if (result.LogFilePath != null) msg += $"\nLog: {result.LogFilePath}";
                MessageBox.Show(msg, result.Success ? "Kurulum Başarılı" : "Kurulum Başarısız",
                    MessageBoxButton.OK, result.Success ? MessageBoxImage.Information : MessageBoxImage.Error);
            }
            finally
            {
                btn.IsEnabled = true;
            }
        }

        private void BrowseGameFolder_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFolderDialog { Title = "Oyun klasörünü seçin" };
            if (dialog.ShowDialog() == true)
                MessageBox.Show($"Seçilen klasör:\n{dialog.FolderName}", "Klasör Seçildi", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private static string GetAppDataPath(string subFolder)
        {
            return System.IO.Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "RenPyTRLauncher", subFolder);
        }

        private void OpenAppFolder(string subFolder)
        {
            var path = GetAppDataPath(subFolder);
            System.IO.Directory.CreateDirectory(path);
            Process.Start(new ProcessStartInfo { FileName = path, UseShellExecute = true });
        }

        private void ClearFolder(string subFolder, string label)
        {
            var path = GetAppDataPath(subFolder);
            if (!System.IO.Directory.Exists(path))
            {
                MessageBox.Show($"{label} klasörü zaten boş.", label, MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (MessageBox.Show($"{label} klasöründeki dosyalar silinsin mi?\n{path}", label,
                    MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes) return;

            foreach (var file in System.IO.Directory.GetFiles(path))
            {
                try { System.IO.File.Delete(file); } catch { }
            }
            MessageBox.Show($"{label} temizlendi.", label, MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ShowAdmin()
        {
            AdminHost.Content = new Views.AdminUserControl();
            AdminHost.Visibility = Visibility.Visible;
            HideAllPages();
            HighlightSidebar(null);
        }

        private void SetActivePage(string pageName)
        {
            _activePage = pageName;
            AdminHost.Visibility = Visibility.Collapsed;
            HideAllPages();

            switch (pageName)
            {
                case "PageAnaSayfa": PageAnaSayfa.Visibility = Visibility.Visible; break;
                case "PageOyunlar": PageOyunlar.Visibility = Visibility.Visible; break;
                case "PageKategori": PageKategori.Visibility = Visibility.Visible; break;
                case "PageTop10": PageTop10.Visibility = Visibility.Visible; break;
                case "PageVip": PageVip.Visibility = Visibility.Visible; break;
                case "PageProfil": PageProfil.Visibility = Visibility.Visible; break;
                case "PageAyarlar": PageAyarlar.Visibility = Visibility.Visible; break;
            }
            HighlightSidebar(pageName);
        }

        private void HighlightSidebar(string? pageName)
        {
            var accent = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(155, 89, 255));
            var activeBg = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(42, 32, 64));
            var inactiveFg = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(184, 184, 192));

            foreach (var (key, btn) in _sidebarMap)
            {
                var active = key == pageName;
                btn.Background = active ? activeBg : System.Windows.Media.Brushes.Transparent;
                btn.Foreground = active ? accent : inactiveFg;
            }
        }

        private void HideAllPages()
        {
            PageAnaSayfa.Visibility = Visibility.Collapsed;
            PageOyunlar.Visibility = Visibility.Collapsed;
            PageKategori.Visibility = Visibility.Collapsed;
            PageTop10.Visibility = Visibility.Collapsed;
            PageVip.Visibility = Visibility.Collapsed;
            PageProfil.Visibility = Visibility.Collapsed;
            PageAyarlar.Visibility = Visibility.Collapsed;
        }

        private void BtnLogout_Click(object? sender, RoutedEventArgs e)
        {
            if (!ConfirmExit()) return;
            Close();
        }

        private void Window_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!ConfirmExit())
                e.Cancel = true;
        }

        private bool ConfirmExit()
        {
            var msg = "Çıkmak istediğinize emin misiniz?";
            if (Services.DownloadTracker.HasActiveDownloads)
                msg = "Devam eden indirme mevcut. Çıkarsanız işlem yarım kalabilir.\n\nÇıkmak istediğinize emin misiniz?";

            return MessageBox.Show(msg, "Çıkış", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes;
        }
    }
}
