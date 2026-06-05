using System;
using System.Linq;
using System.Windows;
using Microsoft.EntityFrameworkCore;

namespace RenPyTRLauncher
{
    public partial class MainWindow : Window
    {
        private readonly ViewModels.MainViewModel viewModel;

        public MainWindow()
        {
            InitializeComponent();

            // initialize EF DbContext and services (EnsureCreated/SaveChanges calls removed to avoid build-time dependency issues)
            var db = new Data.AppDbContext();

            // Try to apply pending EF Core migrations at startup to ensure schema matches model
            try
            {
                db.Database.Migrate();
            }
            catch
            {
                // If migrations cannot be applied at runtime, continue with best-effort schema fixes below
            }

            // Best-effort runtime schema fix: if the Users table is missing the text columns
            // mapped to List<Guid> properties, add them so EF queries don't fail.
            try
            {
                db.Database.OpenConnection();
                using (var cmd = db.Database.GetDbConnection().CreateCommand())
                {
                    cmd.CommandText = "PRAGMA table_info(Users);";
                    var existing = new System.Collections.Generic.HashSet<string>(StringComparer.OrdinalIgnoreCase);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read()) existing.Add(reader.GetString(1));
                    }

                    var needed = new[] { "FavoriteGameIds", "DownloadedPatchIds", "RecentDownloadedGameIds", "TotalDownloadCount" };
                    foreach (var col in needed)
                    {
                        if (!existing.Contains(col))
                        {
                            using (var c2 = db.Database.GetDbConnection().CreateCommand())
                            {
                                // for numeric TotalDownloadCount add INTEGER with default 0
                                if (string.Equals(col, "TotalDownloadCount", System.StringComparison.OrdinalIgnoreCase))
                                {
                                    c2.CommandText = $"ALTER TABLE Users ADD COLUMN {col} INTEGER NOT NULL DEFAULT 0";
                                }
                                else
                                {
                                    c2.CommandText = $"ALTER TABLE Users ADD COLUMN {col} TEXT";
                                }
                                c2.ExecuteNonQuery();
                            }
                        }
                    }
                }
            }
            catch
            {
                // Best-effort: if anything fails, ignore so app can continue to start.
            }
            finally
            {
                try { db.Database.CloseConnection(); } catch { }
            }

            Services.ServiceLocator.GameService = new Services.EfGameService(db);
            Services.ServiceLocator.AnnouncementService = new Services.EfAnnouncementService(db);
            Services.ServiceLocator.UserService = new Services.EfUserService(db);

            viewModel = new ViewModels.MainViewModel();
            DataContext = viewModel;

            // Sidebar handlers via FindName
            var btnAna = GetElement<System.Windows.Controls.Button>("BtnAnaSayfa");
            var btnOyun = GetElement<System.Windows.Controls.Button>("BtnOyunlar");
            var btnKategori = GetElement<System.Windows.Controls.Button>("BtnKategori");
            var btnTop10 = GetElement<System.Windows.Controls.Button>("BtnTop10");
            var btnVip = GetElement<System.Windows.Controls.Button>("BtnVip");
            var btnProfil = GetElement<System.Windows.Controls.Button>("BtnProfil");
            var btnAyar = GetElement<System.Windows.Controls.Button>("BtnAyarlar");
            var btnAdmin = GetElement<System.Windows.Controls.Button>("BtnAdmin");
            var btnLogout = GetElement<System.Windows.Controls.Button>("BtnLogout");

            if (btnAna != null) btnAna.Click += BtnAnaSayfa_Click;
            if (btnOyun != null) btnOyun.Click += BtnOyunlar_Click;
            if (btnKategori != null) btnKategori.Click += BtnKategori_Click;
            if (btnTop10 != null) btnTop10.Click += BtnTop10_Click;
            if (btnVip != null) btnVip.Click += BtnVip_Click;
            if (btnProfil != null) btnProfil.Click += BtnProfil_Click;
            if (btnAyar != null) btnAyar.Click += BtnAyarlar_Click;
            if (btnAdmin != null) btnAdmin.Click += BtnAdmin_Click;
            if (btnLogout != null) btnLogout.Click += BtnLogout_Click;

            // Category buttons (from PageKategori)
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

            // wire category panel controls
            var txtSearch = GetElement<System.Windows.Controls.TextBox>("TxtKategoriSearch");
            var btnClear = GetElement<System.Windows.Controls.Button>("BtnClearCategories");
            var btnApply = GetElement<System.Windows.Controls.Button>("BtnApplyCategories");

            if (txtSearch != null) txtSearch.TextChanged += (s, e) => { viewModel.SearchText = txtSearch.Text; };
            if (btnClear != null) btnClear.Click += (s, e) => { viewModel.ClearCategorySelection(); /* clear UI toggles */ ClearCategoryToggles(); };
            if (btnApply != null) btnApply.Click += (s, e) => { viewModel.ApplyFilters(); SetActivePage("PageKategori"); };
        }

        private void CategoryToggle_Click(object sender, RoutedEventArgs e)
        {
            if (sender is System.Windows.Controls.Primitives.ToggleButton tb && tb.Content is string cat)
            {
                viewModel.ToggleCategorySelection(cat);
            }
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

        // Kaldırılan "Yama Kurulumu" bölümüne ait event handler'lar temizlendi.

        // Kategori butonu handler
        public void BtnKategori_Click(object sender, System.Windows.RoutedEventArgs e) => SetActivePage("PageKategori");
        private void BtnAdmin_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            ShowAdmin();
        }

        private void ShowAdmin()
        {
            // load control into AdminHost
            var ctrl = new Views.AdminUserControl();
            var adminHost = this.FindName("AdminHost") as System.Windows.Controls.ContentControl;
            if (adminHost != null)
            {
                adminHost.Content = ctrl;
                adminHost.Visibility = System.Windows.Visibility.Visible;
            }

            // hide other pages simple approach
            SetVisibility("PageAnaSayfa", Visibility.Collapsed);
            SetVisibility("PageOyunlar", Visibility.Collapsed);
            SetVisibility("PageTop10", Visibility.Collapsed);
            SetVisibility("PageVip", Visibility.Collapsed);
            SetVisibility("PageProfil", Visibility.Collapsed);
            SetVisibility("PageAyarlar", Visibility.Collapsed);
            SetVisibility("PageKategori", Visibility.Collapsed);
        }
        // Centralized page switcher to reduce repetition and ensure consistent title updates
        private void SetActivePage(string pageName)
        {
            // hide all then show target
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

        // Sidebar handlers map to page names
        public void BtnAnaSayfa_Click(object sender, System.Windows.RoutedEventArgs e) => SetActivePage("PageAnaSayfa");
        public void BtnOyunlar_Click(object sender, System.Windows.RoutedEventArgs e) => SetActivePage("PageOyunlar");
        public void BtnTop10_Click(object sender, System.Windows.RoutedEventArgs e) => SetActivePage("PageTop10");
        private void BtnVip_Click(object sender, RoutedEventArgs e) => SetActivePage("PageVip");
        public void BtnProfil_Click(object sender, System.Windows.RoutedEventArgs e) => SetActivePage("PageProfil");
        public void BtnAyarlar_Click(object sender, System.Windows.RoutedEventArgs e) => SetActivePage("PageAyarlar");

        private void BtnLogout_Click(object? sender, RoutedEventArgs e)
        {
            var res = System.Windows.MessageBox.Show("Oturumu kapatmak istediğinize emin misiniz?", "Çıkış", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (res == MessageBoxResult.Yes)
            {
                // For now, just close the main window (login screen can be shown here if exists)
                this.Close();
            }
        }

        // helper to find named element
        private T? GetElement<T>(string name) where T : class
        {
            return this.FindName(name) as T;
        }

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
            // show only games that have PatchFilePath not empty
            vm.FilteredGames = new System.Collections.ObjectModel.ObservableCollection<Models.Game>(vm.Games.Where(g => !string.IsNullOrWhiteSpace(g.PatchFilePath)));
            SetActivePage("PageKategori");
            SetText("TxtSayfa", "Yamalar");
        }
    }
}