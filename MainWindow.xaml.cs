using System;

using System.Diagnostics;
using System.Linq;
using System.Windows;

using System.Windows.Controls;

using System.Windows.Input;

using System.Windows.Media;

using Microsoft.Win32;

using RenPyTRLauncher.Models;

using RenPyTRLauncher.Services;

using RenPyTRLauncher.ViewModels;

using RenPyTRLauncher.Views;



namespace RenPyTRLauncher

{

    public partial class MainWindow : Window

    {

        private readonly MainViewModel viewModel;

        private readonly System.Collections.Generic.Dictionary<string, Button> _sidebarMap = new();



        public MainWindow()

        {
            InitializeComponent();

            viewModel = new MainViewModel();

            DataContext = viewModel;

            ServiceLocator.DataChanged += () => Dispatcher.Invoke(() =>

            {

                UpdateRoleVisibility();

                ApplyTheme();

            });



            WireSidebarButtons();

            WireSettingsButtons();

            WireProfileButtons();

            UpdateRoleVisibility();

            InitThemeRadioButtons();

            SetActivePage("PageAnaSayfa");



            Dispatcher.BeginInvoke(new Action(() => ApplyTheme()), System.Windows.Threading.DispatcherPriority.Loaded);

        }



        private void InitThemeRadioButtons()

        {

            if (viewModel.CurrentTheme == ThemeService.DiscordDark)

                RbThemeDiscord.IsChecked = true;

            else

                RbThemeSteam.IsChecked = true;

        }



        private void ApplyTheme()

        {

            ThemeService.LoadFromSettings(ServiceLocator.SettingsService);

            var palette = ThemeService.GetPalette(ThemeService.CurrentTheme);



            Background = palette.WindowBg;

            Resources["AccentColor"] = palette.Accent;

            Resources["AccentBrush"] = palette.AccentBrush;

            Resources["SurfaceBrush"] = palette.SurfaceBrush;

            Resources["CardBrush"] = palette.CardBrush;



            if (Content is Grid grid && grid.Children.Count > 0 && grid.Children[0] is Border sidebar)

            {

                sidebar.Background = new SolidColorBrush(palette.SidebarBg);

                sidebar.BorderBrush = new SolidColorBrush(palette.SidebarBorder);

            }

        }



        private void WireSidebarButtons()

        {

            _sidebarMap["PageAnaSayfa"] = BtnAnaSayfa;

            _sidebarMap["PageOyunlar"] = BtnOyunlar;

            _sidebarMap["PageFavoriler"] = BtnFavoriler;

            _sidebarMap["PageKategori"] = BtnKategori;


            _sidebarMap["PageVip"] = BtnVip;

            _sidebarMap["PageProfil"] = BtnProfil;

            _sidebarMap["PageAyarlar"] = BtnAyarlar;



            BtnAnaSayfa.Click += (_, _) => SetActivePage("PageAnaSayfa");

            BtnOyunlar.Click += (_, _) => SetActivePage("PageOyunlar");

            BtnFavoriler.Click += (_, _) => SetActivePage("PageFavoriler");

            BtnKategori.Click += (_, _) => { viewModel.CloseCategoryFolder(); SetActivePage("PageKategori"); };


            BtnVip.Click += (_, _) => SetActivePage("PageVip");

            BtnProfil.Click += (_, _) => SetActivePage("PageProfil");

            BtnAyarlar.Click += (_, _) => SetActivePage("PageAyarlar");

            BtnAdmin.Click += (_, _) => ShowAdmin();

            BtnLogout.Click += BtnLogout_Click;

        }






        private void WireProfileButtons()

        {

            BtnEditProfile.Click += (_, _) =>

            {

                if (viewModel.CurrentUser == null) return;

                new EditProfileWindow(viewModel.CurrentUser).ShowDialog();

                ServiceLocator.NotifyDataChanged();

            };

            BtnChangePassword.Click += (_, _) =>

            {

                if (viewModel.CurrentUser == null) return;

                new ChangePasswordWindow(viewModel.CurrentUser.Id).ShowDialog();

            };

            BtnChangeAvatar.Click += BtnChangeAvatar_Click;

        }



        private void BtnChangeAvatar_Click(object? sender, RoutedEventArgs e)

        {

            if (viewModel.CurrentUser == null) return;

            var dlg = new OpenFileDialog { Filter = "Resim|*.png;*.jpg;*.jpeg;*.webp;*.bmp" };

            if (dlg.ShowDialog() != true) return;

            try

            {

                var path = ImageService.UploadFromFile(dlg.FileName, ImageCategory.Avatars);

                viewModel.CurrentUser.AvatarPath = path;

                ServiceLocator.UserService?.Update(viewModel.CurrentUser);

                ServiceLocator.NotifyDataChanged();

            }

            catch (Exception ex)

            {

                MessageBox.Show(ex.Message, "Avatar Yükleme", MessageBoxButton.OK, MessageBoxImage.Warning);

            }

        }






        private void UpdateRoleVisibility()

        {

            BtnAdmin.Visibility = AuthorizationService.CanAccessAdminPanel(viewModel.CurrentUser)

                ? Visibility.Visible : Visibility.Collapsed;

        }



        private void WireSettingsButtons()

        {

            var settingsButtons = new (Button btn, Action action)[]

            {

                (BtnCheckUpdates, () => MessageBox.Show("Güncelleme kontrolü: En son sürüm kullanılıyor (v1.2).", "Güncellemeler", MessageBoxButton.OK, MessageBoxImage.Information)),

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

                var latest = RollbackService.GetLatestBackup();

                if (latest == null)

                {

                    MessageBox.Show("Geri alınacak yedek bulunamadı.", "Rollback", MessageBoxButton.OK, MessageBoxImage.Information);

                    return;

                }

                if (MessageBox.Show($"Son yedek geri yüklensin mi?\n{latest}", "Rollback",

                        MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes) return;



                var result = RollbackService.RollbackFromFolder(latest);

                MessageBox.Show(result.Message, result.Success ? "Rollback Başarılı" : "Rollback Başarısız",

                    MessageBoxButton.OK, result.Success ? MessageBoxImage.Information : MessageBoxImage.Error);

            };

        }



        // private void CmbSearchCategory_Changed(object sender, SelectionChangedEventArgs e)
        // {
        //     if (CmbSearchCategory.SelectedItem is string cat)
        //         viewModel.SearchCategoryFilter = cat;
        // }

        // private void ChkSearchFavorites_Changed(object sender, RoutedEventArgs e) =>
        //     viewModel.SearchInFavoritesOnly = ChkSearchFavorites.IsChecked == true;



        private void BtnNotifications_Click(object sender, RoutedEventArgs e) =>

            NotificationPanel.Visibility = NotificationPanel.Visibility == Visibility.Visible

                ? Visibility.Collapsed : Visibility.Visible;



        private void MarkAllNotifications_Click(object sender, RoutedEventArgs e)

        {

            viewModel.MarkAllNotificationsRead();

            NotificationPanel.Visibility = Visibility.Collapsed;

        }



        private void NotificationItem_Click(object sender, MouseButtonEventArgs e)

        {

            if (sender is FrameworkElement fe && fe.DataContext is Notification n)

            {

                viewModel.MarkNotificationRead(n);

                if (n.RelatedGameId.HasValue)

                {

                    var game = viewModel.Games.FirstOrDefault(g => g.Id == n.RelatedGameId.Value);

                    if (game != null) OpenGameDetail(game);

                }

            }

        }



        private void ThemeChanged(object sender, RoutedEventArgs e)

        {

            if (RbThemeDiscord.IsChecked == true)

                viewModel.SetTheme(ThemeService.DiscordDark);

            else if (RbThemeSteam.IsChecked == true)

                viewModel.SetTheme(ThemeService.SteamDark);

            ApplyTheme();

        }



        private void GameDetail_Click(object sender, RoutedEventArgs e)

        {

            if (sender is Button btn && btn.Tag is Game game)

                OpenGameDetail(game);

        }



        private void GameCard_Click(object sender, MouseButtonEventArgs e)

        {

            if (sender is FrameworkElement fe && fe.Tag is Game game)

                OpenGameDetail(game);

        }



        private void FavoriteGame_Click(object sender, MouseButtonEventArgs e)

        {

            if (sender is FrameworkElement fe && fe.Tag is Game game)

                OpenGameDetail(game);

        }



        private void RemoveFavorite_Click(object sender, RoutedEventArgs e)

        {

            if (sender is Button btn && btn.Tag is Game game)

                viewModel.ToggleFavorite(game);

        }



        private void OpenGameDetail(Game game)

        {

            new GameDetailWindow(game, viewModel) { Owner = this }.ShowDialog();

        }



        private void AnnPrev_Click(object sender, RoutedEventArgs e) => viewModel.PrevAnnouncement();

        private void AnnNext_Click(object sender, RoutedEventArgs e) => viewModel.NextAnnouncement();



        private void CategoryFolder_Click(object sender, RoutedEventArgs e)

        {

            if (sender is Button btn && btn.Tag is CategoryFolderItem folder)

                viewModel.OpenCategoryFolder(folder.CategoryKey, folder.DisplayName);

        }



        private void CategoryBack_Click(object sender, RoutedEventArgs e) => viewModel.CloseCategoryFolder();



        private void PurchaseMembership_Click(object sender, RoutedEventArgs e)

        {

            if (sender is Button btn && btn.Tag is string url)

                OpenUrl(url);

        }



        private void OpenLink_Click(object sender, RoutedEventArgs e)

        {

            if (sender is Button btn && btn.Tag is string url)

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

            if (sender is not Button btn) return;

            var game = btn.Tag as Game ?? (btn.Tag as LeaderboardEntry)?.Game;

            if (game == null) return;



            if (viewModel.CurrentUser == null)

            {

                MessageBox.Show("Kullanıcı oturumu bulunamadı.", "Yama Kur", MessageBoxButton.OK, MessageBoxImage.Warning);

                return;

            }



            if (!AuthorizationService.CanInstallVipPatch(viewModel.CurrentUser, game))

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



        private static string GetAppDataPath(string subFolder) =>

            System.IO.Path.Combine(

                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),

                "RenPyTRLauncher", subFolder);



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

            if (!AuthorizationService.CanAccessAdminPanel(viewModel.CurrentUser))

            {

                MessageBox.Show("Bu alana erişim yetkiniz yok.", "Yetkisiz", MessageBoxButton.OK, MessageBoxImage.Warning);

                return;

            }

            AdminHost.Content = new AdminUserControl(viewModel.CurrentUser);

            AdminHost.Visibility = Visibility.Visible;

            HideAllPages();

            HighlightSidebar(null);

        }



        private void SetActivePage(string pageName)

        {

            NotificationPanel.Visibility = Visibility.Collapsed;

            AdminHost.Visibility = Visibility.Collapsed;

            HideAllPages();



            switch (pageName)

            {

                case "PageAnaSayfa": PageAnaSayfa.Visibility = Visibility.Visible; break;

                case "PageOyunlar": PageOyunlar.Visibility = Visibility.Visible; break;

                case "PageFavoriler": PageFavoriler.Visibility = Visibility.Visible; break;

                case "PageKategori": PageKategori.Visibility = Visibility.Visible; break;

                case "PageVip": PageVip.Visibility = Visibility.Visible; break;

                case "PageProfil": PageProfil.Visibility = Visibility.Visible; break;

                case "PageAyarlar":

                    PageAyarlar.Visibility = Visibility.Visible;

                    InitThemeRadioButtons();

                    break;

            }

            HighlightSidebar(pageName);

        }



        private void HighlightSidebar(string? pageName)

        {

            var palette = ThemeService.GetPalette(ThemeService.CurrentTheme);

            var accent = palette.AccentBrush;

            var activeBg = new SolidColorBrush(palette.SidebarActiveBg);

            var inactiveFg = new SolidColorBrush(palette.TextMuted);



            foreach (var (key, btn) in _sidebarMap)

            {

                var active = key == pageName;

                btn.Background = active ? activeBg : Brushes.Transparent;

                btn.Foreground = active ? accent : inactiveFg;

            }

        }



        private void HideAllPages()

        {

            PageAnaSayfa.Visibility = Visibility.Collapsed;

            PageOyunlar.Visibility = Visibility.Collapsed;

            PageFavoriler.Visibility = Visibility.Collapsed;

            PageKategori.Visibility = Visibility.Collapsed;

            PageVip.Visibility = Visibility.Collapsed;

            PageProfil.Visibility = Visibility.Collapsed;

            PageAyarlar.Visibility = Visibility.Collapsed;

        }






        private void Window_Closing(object? sender, System.ComponentModel.CancelEventArgs e)

        {

            if (!ConfirmExit())

                e.Cancel = true;

        }



        private bool ConfirmExit()

        {

            var msg = "Çıkmak istediğinize emin misiniz?";

            if (DownloadTracker.HasActiveDownloads)

                msg = "Devam eden indirme mevcut. Çıkarsanız işlem yarım kalabilir.\n\nÇıkmak istediğinize emin misiniz?";



            return MessageBox.Show(msg, "Çıkış", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes;

        }



        private void BtnLogout_Click(object? sender, RoutedEventArgs e)

        {

            if (MessageBox.Show("Oturumu kapatmak istediğinize emin misiniz?", "Çıkış",

                    MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes) return;



            ServiceLocator.AuthService?.Logout();

            var login = new LoginWindow();

            login.Show();

            Close();

        }

    }

}


