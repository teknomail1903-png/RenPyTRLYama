using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Win32;
using RenPyTRLauncher.Models;

namespace RenPyTRLauncher
{
    public partial class MainWindow : Window
    {
        private readonly ViewModels.MainViewModel viewModel;

        public MainWindow()
        {
            InitializeComponent();

            var db = Data.DatabaseInitializer.Initialize();

            Services.ServiceLocator.GameService = new Services.EfGameService(db);
            Services.ServiceLocator.AnnouncementService = new Services.EfAnnouncementService(db);
            Services.ServiceLocator.UserService = new Services.EfUserService(db);
            Services.ServiceLocator.PatchService = new Services.PatchService(
                Services.ServiceLocator.GameService,
                Services.ServiceLocator.UserService);

            viewModel = new ViewModels.MainViewModel();
            DataContext = viewModel;

            WireSidebarButtons();
            WireCategoryButtons();
            WireSettingsButtons();
        }

        private void WireSidebarButtons()
        {
            var mappings = new (string name, RoutedEventHandler handler)[]
            {
                ("BtnAnaSayfa", BtnAnaSayfa_Click),
                ("BtnOyunlar", BtnOyunlar_Click),
                ("BtnKategori", BtnKategori_Click),
                ("BtnTop10", BtnTop10_Click),
                ("BtnVip", BtnVip_Click),
                ("BtnProfil", BtnProfil_Click),
                ("BtnAyarlar", BtnAyarlar_Click),
                ("BtnAdmin", BtnAdmin_Click),
                ("BtnLogout", BtnLogout_Click)
            };

            foreach (var (name, handler) in mappings)
            {
                var btn = GetElement<System.Windows.Controls.Button>(name);
                if (btn != null) btn.Click += handler;
            }
        }

        private void WireCategoryButtons()
        {
            var btnKategoriVip = GetElement<System.Windows.Controls.Button>("BtnKategoriVip");
            var btnKategoriDevam = GetElement<System.Windows.Controls.Button>("BtnKategoriDevam");
            var btnKategoriBiten = GetElement<System.Windows.Controls.Button>("BtnKategoriBiten");
            var btnKategoriErkek = GetElement<System.Windows.Controls.Button>("BtnKategoriErkek");
            var btnKategoriKadin = GetElement<System.Windows.Controls.Button>("BtnKategoriKadin");
            var btnKategoriYamalar = GetElement<System.Windows.Controls.Button>("BtnKategoriYamalar");

            if (btnKategoriVip != null) btnKategoriVip.Click += (s, e) => viewModel.FilterByCategory("VIP");
            if (btnKategoriDevam != null) btnKategoriDevam.Click += (s, e) => viewModel.FilterByCategory("Devam Eden");
            if (btnKategoriBiten != null) btnKategoriBiten.Click += (s, e) => viewModel.FilterByCategory("Biten");
            if (btnKategoriErkek != null) btnKategoriErkek.Click += (s, e) => viewModel.FilterByCategory("Erkek Başrol");
            if (btnKategoriKadin != null) btnKategoriKadin.Click += (s, e) => viewModel.FilterByCategory("Kadın Başrol");
            if (btnKategoriYamalar != null) btnKategoriYamalar.Click += (s, e) => FilterByPatches(viewModel);

            var txtSearch = GetElement<System.Windows.Controls.TextBox>("TxtKategoriSearch");
            var btnClear = GetElement<System.Windows.Controls.Button>("BtnClearCategories");
            var btnApply = GetElement<System.Windows.Controls.Button>("BtnApplyCategories");

            if (txtSearch != null) txtSearch.TextChanged += (s, e) => { viewModel.SearchText = txtSearch.Text; viewModel.ApplyFilters(); };
            if (btnClear != null) btnClear.Click += (s, e) => { viewModel.ClearCategorySelection(); ClearCategoryToggles(); };
            if (btnApply != null) btnApply.Click += (s, e) => { viewModel.ApplyFilters(); SetActivePage("PageKategori"); };
        }

        private void WireSettingsButtons()
        {
            var settingsButtons = new (string name, Action action)[]
            {
                ("BtnCheckUpdates", () => MessageBox.Show("Güncelleme kontrolü: En son sürüm kullanılıyor (v1.0).", "Güncellemeler", MessageBoxButton.OK, MessageBoxImage.Information)),
                ("BtnClearSaves", () => ClearFolder("Saves", "Kayıt dosyaları")),
                ("BtnClearCache", () => ClearFolder("Cache", "Önbellek")),
                ("BtnClearLogs", () => ClearFolder("Logs", "Log dosyaları")),
                ("BtnOpenGamesFolder", () => OpenAppFolder("Games")),
                ("BtnOpenBackupFolder", () => OpenAppFolder("Backups"))
            };

            foreach (var (name, action) in settingsButtons)
            {
                var btn = GetElement<System.Windows.Controls.Button>(name);
                if (btn != null) btn.Click += (s, e) => action();
            }
        }

        private async void InstallPatch_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not System.Windows.Controls.Button btn || btn.Tag is not Game game)
                return;

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
                MessageBox.Show(
                    result.Message + (result.LogFilePath != null ? $"\n\nLog: {result.LogFilePath}" : string.Empty),
                    result.Success ? "Kurulum Başarılı" : "Kurulum Başarısız",
                    MessageBoxButton.OK,
                    result.Success ? MessageBoxImage.Information : MessageBoxImage.Error);
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
            var basePath = System.IO.Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "RenPyTRLauncher");
            return System.IO.Path.Combine(basePath, subFolder);
        }

        private void OpenAppFolder(string subFolder)
        {
            var path = GetAppDataPath(subFolder);
            System.IO.Directory.CreateDirectory(path);
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = path,
                UseShellExecute = true
            });
        }

        private void ClearFolder(string subFolder, string label)
        {
            var path = GetAppDataPath(subFolder);
            if (!System.IO.Directory.Exists(path))
            {
                MessageBox.Show($"{label} klasörü zaten boş.", label, MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var res = MessageBox.Show($"{label} klasöründeki dosyalar silinsin mi?\n{path}", label, MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (res != MessageBoxResult.Yes) return;

            foreach (var file in System.IO.Directory.GetFiles(path))
            {
                try { System.IO.File.Delete(file); } catch { }
            }
            MessageBox.Show($"{label} temizlendi.", label, MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void CategoryToggle_Click(object sender, RoutedEventArgs e)
        {
            if (sender is System.Windows.Controls.Primitives.ToggleButton tb && tb.Content is string cat)
                viewModel.ToggleCategorySelection(cat);
        }

        private void ClearCategoryToggles()
        {
            var container = GetElement<System.Windows.Controls.ItemsControl>("CategoryList");
            if (container == null) return;
            foreach (var item in container.Items)
            {
                var c = container.ItemContainerGenerator.ContainerFromItem(item) as System.Windows.DependencyObject;
                if (c == null) continue;
                var toggle = FindVisualChild<System.Windows.Controls.Primitives.ToggleButton>(c);
                if (toggle != null) toggle.IsChecked = false;
            }
        }

        private static T? FindVisualChild<T>(System.Windows.DependencyObject parent) where T : System.Windows.DependencyObject
        {
            if (parent == null) return null;
            var count = System.Windows.Media.VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < count; i++)
            {
                var child = System.Windows.Media.VisualTreeHelper.GetChild(parent, i);
                if (child is T t) return t;
                var res = FindVisualChild<T>(child);
                if (res != null) return res;
            }
            return null;
        }

        public void BtnKategori_Click(object sender, RoutedEventArgs e) => SetActivePage("PageKategori");

        private void BtnAdmin_Click(object sender, RoutedEventArgs e) => ShowAdmin();

        private void ShowAdmin()
        {
            var ctrl = new Views.AdminUserControl();
            var adminHost = this.FindName("AdminHost") as System.Windows.Controls.ContentControl;
            if (adminHost != null)
            {
                adminHost.Content = ctrl;
                adminHost.Visibility = Visibility.Visible;
            }

            SetVisibility("PageAnaSayfa", Visibility.Collapsed);
            SetVisibility("PageOyunlar", Visibility.Collapsed);
            SetVisibility("PageTop10", Visibility.Collapsed);
            SetVisibility("PageVip", Visibility.Collapsed);
            SetVisibility("PageProfil", Visibility.Collapsed);
            SetVisibility("PageAyarlar", Visibility.Collapsed);
            SetVisibility("PageKategori", Visibility.Collapsed);
        }

        private void SetActivePage(string pageName)
        {
            var adminHost = GetElement<System.Windows.Controls.ContentControl>("AdminHost");
            if (adminHost != null) adminHost.Visibility = Visibility.Collapsed;

            SetVisibility("PageAnaSayfa", Visibility.Collapsed);
            SetVisibility("PageOyunlar", Visibility.Collapsed);
            SetVisibility("PageTop10", Visibility.Collapsed);
            SetVisibility("PageVip", Visibility.Collapsed);
            SetVisibility("PageProfil", Visibility.Collapsed);
            SetVisibility("PageAyarlar", Visibility.Collapsed);
            SetVisibility("PageKategori", Visibility.Collapsed);

            switch (pageName)
            {
                case "PageAnaSayfa":
                    SetVisibility("PageAnaSayfa", Visibility.Visible);
                    SetText("TxtSayfa", "Ana Sayfa");
                    break;
                case "PageOyunlar":
                    SetVisibility("PageOyunlar", Visibility.Visible);
                    SetText("TxtSayfa", "Oyunlar");
                    break;
                case "PageTop10":
                    SetVisibility("PageTop10", Visibility.Visible);
                    SetText("TxtSayfa", "Top 10");
                    break;
                case "PageVip":
                    SetVisibility("PageVip", Visibility.Visible);
                    SetText("TxtSayfa", "VIP Oyunlar");
                    break;
                case "PageKategori":
                    SetVisibility("PageKategori", Visibility.Visible);
                    SetText("TxtSayfa", "Kategoriler");
                    break;
                case "PageProfil":
                    SetVisibility("PageProfil", Visibility.Visible);
                    SetText("TxtSayfa", "Profil");
                    break;
                case "PageAyarlar":
                    SetVisibility("PageAyarlar", Visibility.Visible);
                    SetText("TxtSayfa", "Ayarlar");
                    break;
            }
        }

        public void BtnAnaSayfa_Click(object sender, RoutedEventArgs e) => SetActivePage("PageAnaSayfa");
        public void BtnOyunlar_Click(object sender, RoutedEventArgs e) => SetActivePage("PageOyunlar");
        public void BtnTop10_Click(object sender, RoutedEventArgs e) => SetActivePage("PageTop10");
        private void BtnVip_Click(object sender, RoutedEventArgs e) => SetActivePage("PageVip");
        public void BtnProfil_Click(object sender, RoutedEventArgs e) => SetActivePage("PageProfil");
        public void BtnAyarlar_Click(object sender, RoutedEventArgs e) => SetActivePage("PageAyarlar");

        private void BtnLogout_Click(object? sender, RoutedEventArgs e)
        {
            var res = MessageBox.Show("Oturumu kapatmak istediğinize emin misiniz?", "Çıkış", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (res == MessageBoxResult.Yes) Close();
        }

        private T? GetElement<T>(string name) where T : class => FindName(name) as T;

        private void SetVisibility(string name, Visibility visibility)
        {
            var el = GetElement<System.Windows.FrameworkElement>(name);
            if (el != null) el.Visibility = visibility;
        }

        private void SetText(string name, string text)
        {
            var tb = GetElement<System.Windows.Controls.TextBlock>(name);
            if (tb != null) tb.Text = text;
        }

        private void FilterByPatches(ViewModels.MainViewModel vm)
        {
            if (vm == null) return;
            vm.FilteredGames = new System.Collections.ObjectModel.ObservableCollection<Game>(
                vm.Games.Where(g => !string.IsNullOrWhiteSpace(g.PatchFilePath)));
            SetActivePage("PageKategori");
            SetText("TxtSayfa", "Yamalar");
        }
    }
}
