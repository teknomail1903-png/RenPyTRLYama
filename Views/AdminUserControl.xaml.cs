using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using RenPyTRLauncher.Models;
using RenPyTRLauncher.Services;

namespace RenPyTRLauncher.Views
{
    public partial class AdminUserControl : UserControl
    {
        private readonly IGameService _gameService;
        private readonly IAnnouncementService _announcementService;
        private readonly IUserService _userService;
        private readonly ISettingsService _settingsService;
        private readonly IMembershipService _membershipService;
        private readonly ISupportService _supportService;
        private readonly ICategoryService _categoryService;
        private readonly IHelpService _helpService;
        private readonly INotificationService _notificationService;
        private readonly User? _currentUser;

        public AdminUserControl(User? currentUser = null)
        {
            try
            {
                App.Log("AdminUserControl Constructor Start");
                InitializeComponent();
                _currentUser = currentUser;

                _gameService = ServiceLocator.GameService ?? new InMemoryGameService();
                _announcementService = ServiceLocator.AnnouncementService ?? new InMemoryAnnouncementService();
                _userService = ServiceLocator.UserService ?? new InMemoryUserService();
                App.Log("AdminUserControl Constructor End");
            }
            catch (Exception ex)
            {
                App.Log($"AdminUserControl Constructor Error: {ex.Message}");
                App.Log($"AdminUserControl Constructor StackTrace: {ex.StackTrace}");
                MessageBox.Show($"AdminUserControl oluşturulurken hata oluştu:\n\n{ex.Message}\n\nDetaylar için logs/startup.log dosyasını kontrol edin.", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
                throw;
            }
            _settingsService = ServiceLocator.SettingsService ?? new InMemorySettingsService();
            _membershipService = ServiceLocator.MembershipService ?? new InMemoryMembershipService();
            _supportService = ServiceLocator.SupportService ?? new InMemorySupportService();
            _categoryService = ServiceLocator.CategoryService ?? new InMemoryCategoryService();
            _helpService = ServiceLocator.HelpService ?? new InMemoryHelpService();
            _notificationService = ServiceLocator.NotificationService ?? new InMemoryNotificationService(_userService);

            ServiceLocator.DataChanged += OnDataChanged;

            // Sidebar button events
            BtnDashboard.Click += (_, _) => ShowPage("Dashboard");
            BtnGames.Click += (_, _) => ShowPage("Games");
            BtnPatches.Click += (_, _) => ShowPage("Patches");
            BtnSlider.Click += (_, _) => ShowPage("Slider");
            BtnAnnouncements.Click += (_, _) => ShowPage("Announcements");
            BtnUsers.Click += (_, _) => ShowPage("Users");
            BtnVipMembers.Click += (_, _) => ShowPage("VipMembers");
            BtnMediaLibrary.Click += (_, _) => ShowPage("MediaLibrary");
            BtnSettings.Click += (_, _) => ShowPage("Settings");

            // Game management
            BtnAddGame.Click += BtnAddGame_Click;
            BtnDeleteGame.Click += BtnDeleteGame_Click;
            BtnEditGame.Click += BtnEditGame_Click;

            // Patch management
            BtnAddPatch.Click += BtnAddPatch_Click;
            BtnDeletePatch.Click += BtnDeletePatch_Click;
            BtnEditPatch.Click += BtnEditPatch_Click;

            // Slider management
            BtnAddToSlider.Click += BtnAddToSlider_Click;
            BtnRemoveFromSlider.Click += BtnRemoveFromSlider_Click;

            // Announcement management
            BtnAddAnn.Click += BtnAddAnn_Click;
            BtnUpdateAnn.Click += BtnUpdateAnn_Click;
            BtnDeleteAnn.Click += BtnDeleteAnn_Click;
            LstAnns.SelectionChanged += (_, _) => LoadSelectedAnnouncement();

            // User management
            BtnUserAdd.Click += BtnUserAdd_Click;
            BtnUserEdit.Click += BtnUserEdit_Click;
            BtnUserDelete.Click += BtnUserDelete_Click;
            BtnGrantVip.Click += BtnGrantVip_Click;
            BtnRevokeVip.Click += BtnRevokeVip_Click;
            BtnExtendVip.Click += BtnExtendVip_Click;

            // VIP membership management
            BtnSaveTier.Click += BtnSaveTier_Click;
            BtnDeleteTier.Click += BtnDeleteTier_Click;

            // Media library
            BtnAddMedia.Click += BtnAddMedia_Click;
            BtnCheckMedia.Click += BtnCheckMedia_Click;

            // Support system
            BtnSupport.Click += BtnSupport_Click;

            // Settings
            BtnSaveSettings.Click += BtnSaveSettings_Click;

            // Quick Actions
            BtnQuickAddGame.Click += BtnAddGame_Click;
            BtnQuickAddAnnouncement.Click += BtnQuickAddAnnouncement_Click;
            BtnQuickSearchUser.Click += BtnQuickSearchUser_Click;
            BtnQuickGiveVip.Click += BtnQuickGiveVip_Click;

            RefreshAll();
        }

        private void ShowPage(string pageName)
        {
            PageDashboard.Visibility = Visibility.Collapsed;
            PageGames.Visibility = Visibility.Collapsed;
            PagePatches.Visibility = Visibility.Collapsed;
            PageSlider.Visibility = Visibility.Collapsed;
            PageAnnouncements.Visibility = Visibility.Collapsed;
            PageUsers.Visibility = Visibility.Collapsed;
            PageVipMembers.Visibility = Visibility.Collapsed;
            PageMediaLibrary.Visibility = Visibility.Collapsed;
            PageSupport.Visibility = Visibility.Collapsed;
            PageSettings.Visibility = Visibility.Collapsed;

            switch (pageName)
            {
                case "Dashboard": PageDashboard.Visibility = Visibility.Visible; break;
                case "Games": PageGames.Visibility = Visibility.Visible; break;
                case "Patches": PagePatches.Visibility = Visibility.Visible; break;
                case "Slider": PageSlider.Visibility = Visibility.Visible; break;
                case "Announcements": PageAnnouncements.Visibility = Visibility.Visible; break;
                case "Users": PageUsers.Visibility = Visibility.Visible; break;
                case "VipMembers": PageVipMembers.Visibility = Visibility.Visible; break;
                case "MediaLibrary": PageMediaLibrary.Visibility = Visibility.Visible; break;
                case "Support": PageSupport.Visibility = Visibility.Visible; break;
                case "Settings": PageSettings.Visibility = Visibility.Visible; break;
            }

            UpdateSidebarButtonStyles(pageName);
        }

        private void UpdateSidebarButtonStyles(string activePage)
        {
            BtnDashboard.Style = (Style)FindResource("SidebarButton");
            BtnGames.Style = (Style)FindResource("SidebarButton");
            BtnPatches.Style = (Style)FindResource("SidebarButton");
            BtnSlider.Style = (Style)FindResource("SidebarButton");
            BtnAnnouncements.Style = (Style)FindResource("SidebarButton");
            BtnUsers.Style = (Style)FindResource("SidebarButton");
            BtnVipMembers.Style = (Style)FindResource("SidebarButton");
            BtnSupport.Style = (Style)FindResource("SidebarButton");
            BtnMediaLibrary.Style = (Style)FindResource("SidebarButton");
            BtnSettings.Style = (Style)FindResource("SidebarButton");

            Button? activeButton = activePage switch
            {
                "Dashboard" => BtnDashboard,
                "Games" => BtnGames,
                "Patches" => BtnPatches,
                "Slider" => BtnSlider,
                "Announcements" => BtnAnnouncements,
                "Users" => BtnUsers,
                "VipMembers" => BtnVipMembers,
                "Support" => BtnSupport,
                "MediaLibrary" => BtnMediaLibrary,
                "Settings" => BtnSettings,
                _ => null
            };

            if (activeButton != null)
            {
                activeButton.Style = (Style)FindResource("SidebarButtonActive");
            }
        }

        private void OnDataChanged() =>
            Dispatcher?.BeginInvoke(new Action(RefreshAll));

        private void ApplyRoleRestrictions()
        {
            var isAdmin = AuthorizationService.CanManageGames(_currentUser);
            var isMod = AuthorizationService.CanManageSupport(_currentUser);

            // Yeni yapıda role restrictions daha sonra eklenecek
        }

        private void RefreshAll()
        {
            RefreshDashboard();
            RefreshLists();
            RefreshPatches();
            RefreshSlider();
            RefreshUsers();
            RefreshMemberships();
            LoadSettingsForm();
        }

        private void RefreshDashboard()
        {
            var games = _gameService.GetAll();
            var patches = games.Where(g => g.Type == GameType.Translation || g.Type == GameType.Gallery || g.Type == GameType.Cheat || g.Type == GameType.Walkthrough || g.Type == GameType.Save || g.Type == GameType.Extra).ToList();
            var users = _userService.GetAll().ToList();
            var vipUsers = users.Where(u => u.IsVip).ToList();
            var announcements = _announcementService.GetAll().ToList();
            var activeAnnouncements = announcements.Where(a => a.IsActive).ToList();

            TxtStatGames.Text = games.Count(g => g.Type == GameType.Game).ToString();
            TxtStatUsers.Text = users.Count.ToString();
            TxtStatDownloads.Text = users.Sum(u => u.TotalDownloadCount).ToString();
            TxtStatAnnouncements.Text = activeAnnouncements.Count.ToString();
            TxtStatVip.Text = vipUsers.Count.ToString();

            // System Status
            TxtDbStatus.Text = "Aktif";
            TxtTotalGames.Text = games.Count(g => g.Type == GameType.Game).ToString();
            TxtTotalPatches.Text = patches.Count.ToString();
            TxtTotalUsers.Text = users.Count.ToString();
            TxtAppVersion.Text = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "1.0.0";

            // Son eklenen içerikler
            var recentContent = games.OrderByDescending(g => g.CreatedDate).Take(5).Select(g => new
            {
                g.Name,
                TypeLabel = g.Type == GameType.Game ? "Oyun" : "Mod",
                AddedDate = g.CreatedDate
            }).ToList();
            RecentContentList.ItemsSource = recentContent;
        }

        private void RefreshLists()
        {
            App.Log("AdminUserControl.RefreshLists - Starting to refresh lists");

            var allGames = _gameService.GetAll().ToList();
            App.Log($"AdminUserControl.RefreshLists - Retrieved {allGames.Count} games from database");

            LstGames.ItemsSource = null;
            LstGames.ItemsSource = allGames;
            App.Log($"AdminUserControl.RefreshLists - LstGames.ItemsSource set with {allGames.Count} games");

            var allAnnouncements = _announcementService.GetAll();
            App.Log($"AdminUserControl.RefreshLists - Retrieved {allAnnouncements.Count()} announcements");

            LstAnns.ItemsSource = null;
            LstAnns.ItemsSource = allAnnouncements;
            App.Log($"AdminUserControl.RefreshLists - LstAnns.ItemsSource set with {allAnnouncements.Count()} announcements");
        }

        private void RefreshPatches()
        {
            App.Log("AdminUserControl.RefreshPatches - Starting to refresh patches list");

            var patches = _gameService.GetAll().Where(g => g.Type == GameType.Translation || g.Type == GameType.Gallery || g.Type == GameType.Cheat || g.Type == GameType.Walkthrough || g.Type == GameType.Save || g.Type == GameType.Extra).ToList();
            App.Log($"AdminUserControl.RefreshPatches - Retrieved {patches.Count} patches from database");

            LstPatches.ItemsSource = null;
            LstPatches.ItemsSource = patches;
            App.Log($"AdminUserControl.RefreshPatches - LstPatches.ItemsSource set with {patches.Count} patches");
        }

        private void RefreshSlider()
        {
            App.Log("AdminUserControl.RefreshSlider - Starting to refresh slider list");

            var sliderItems = _gameService.GetAll().Where(g => g.IsFeatured).ToList();
            App.Log($"AdminUserControl.RefreshSlider - Retrieved {sliderItems.Count} featured items from database");

            LstSlider.ItemsSource = null;
            LstSlider.ItemsSource = sliderItems;
            App.Log($"AdminUserControl.RefreshSlider - LstSlider.ItemsSource set with {sliderItems.Count} items");
        }

        private void RefreshUsers()
        {
            LstUsers.ItemsSource = null;
            LstUsers.ItemsSource = _userService.GetAll();
        }

        private void RefreshMemberships()
        {
            LstMemberships.ItemsSource = null;
            LstMemberships.ItemsSource = _membershipService.GetAll().ToList();
        }

        private void LoadSettingsForm()
        {
            TxtWebsiteUrl.Text = _settingsService.Get(AppSettingKeys.WebsiteUrl);
            TxtDiscordUrl.Text = _settingsService.Get(AppSettingKeys.DiscordUrl);
            TxtAnnouncementsUrl.Text = _settingsService.Get(AppSettingKeys.AnnouncementsUrl);
            TxtSupportUrl.Text = _settingsService.Get(AppSettingKeys.SupportUrl);
        }

        private void LoadSelectedTier()
        {
            if (LstMemberships.SelectedItem is not MembershipTier tier) return;
            TxtTierName.Text = tier.Name;
            TxtTierPrice.Text = tier.PriceLabel;
            TxtTierUrl.Text = tier.PurchaseUrl;
        }

        private void BtnAddMedia_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Medya ekleme özelliği yakında eklenecek.", "Bilgi", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void BtnCheckMedia_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Link kontrol özelliği yakında eklenecek.", "Bilgi", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void BtnSupport_Click(object sender, RoutedEventArgs e)
        {
            ShowPage("Support");
        }

        private void BtnSaveTier_Click(object sender, RoutedEventArgs e)
        {
            if (LstMemberships.SelectedItem is MembershipTier tier)
            {
                tier.Name = TxtTierName.Text;
                tier.PriceLabel = TxtTierPrice.Text;
                tier.PurchaseUrl = TxtTierUrl.Text;
                _membershipService.Update(tier);
            }
            else
            {
                _membershipService.Add(new MembershipTier
                {
                    Name = TxtTierName.Text,
                    PriceLabel = TxtTierPrice.Text,
                    PurchaseUrl = TxtTierUrl.Text,
                    SortOrder = _membershipService.GetAll().Count() + 1
                });
            }
            ServiceLocator.NotifyDataChanged();
            RefreshMemberships();
        }

        private void BtnDeleteTier_Click(object sender, RoutedEventArgs e)
        {
            if (LstMemberships.SelectedItem is MembershipTier tier)
            {
                _membershipService.Remove(tier.Id);
                ServiceLocator.NotifyDataChanged();
                RefreshMemberships();
            }
        }

        private void BtnSaveSettings_Click(object sender, RoutedEventArgs e)
        {
            _settingsService.Set(AppSettingKeys.WebsiteUrl, TxtWebsiteUrl.Text);
            _settingsService.Set(AppSettingKeys.DiscordUrl, TxtDiscordUrl.Text);
            _settingsService.Set(AppSettingKeys.AnnouncementsUrl, TxtAnnouncementsUrl.Text);
            _settingsService.Set(AppSettingKeys.SupportUrl, TxtSupportUrl.Text);
            ServiceLocator.NotifyDataChanged();
            MessageBox.Show("Bağlantı ayarları kaydedildi.", "Ayarlar", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void BtnAddGame_Click(object sender, RoutedEventArgs e)
        {
            new EditGameWindow(null, _gameService).ShowDialog();
            var latest = _gameService.GetAll().OrderByDescending(g => g.CreatedDate).FirstOrDefault();
            if (latest != null)
            {
                _notificationService.NotifyAllUsers(
                    "Yeni Oyun Eklendi",
                    $"{latest.Name} kataloğa eklendi.",
                    NotificationType.NewGame,
                    latest.Id);
            }
            ServiceLocator.NotifyDataChanged();
            RefreshAll();
        }

        private void BtnDeleteGame_Click(object sender, RoutedEventArgs e)
        {
            if (LstGames.SelectedItem is Game g)
            {
                _gameService.Remove(g.Id);
                ServiceLocator.NotifyDataChanged();
                RefreshAll();
            }
        }

        private void BtnEditGame_Click(object sender, RoutedEventArgs e)
        {
            if (LstGames.SelectedItem is Game g)
            {
                System.Diagnostics.Debug.WriteLine($"AdminUserControl.BtnEditGame_Click - Selected Game.Id: {g.Id}, Game.Name: {g.Name}, g.GetHashCode(): {g.GetHashCode()}");
                System.Diagnostics.Debug.WriteLine($"AdminUserControl.BtnEditGame_Click - About to open EditGameWindow with game object");
                new EditGameWindow(g, _gameService).ShowDialog();
                RefreshAll();
            }
        }

        private void BtnAddPatch_Click(object sender, RoutedEventArgs e)
        {
            var patch = new Game { Type = GameType.Translation, Id = Guid.NewGuid() };
            System.Diagnostics.Debug.WriteLine($"AdminUserControl.BtnAddPatch_Click - Created new patch with Id: {patch.Id}");
            new EditGameWindow(patch, _gameService).ShowDialog();
            ServiceLocator.NotifyDataChanged();
            RefreshAll();
        }

        private void BtnDeletePatch_Click(object sender, RoutedEventArgs e)
        {
            if (LstPatches.SelectedItem is Game g)
            {
                _gameService.Remove(g.Id);
                ServiceLocator.NotifyDataChanged();
                RefreshAll();
            }
        }

        private void BtnEditPatch_Click(object sender, RoutedEventArgs e)
        {
            if (LstPatches.SelectedItem is Game g)
            {
                new EditGameWindow(g, _gameService).ShowDialog();
                RefreshAll();
            }
        }

        private void BtnAddToSlider_Click(object sender, RoutedEventArgs e)
        {
            if (LstSlider.SelectedItem is Game g)
            {
                g.IsFeatured = true;
                _gameService.Update(g);
                ServiceLocator.NotifyDataChanged();
                RefreshAll();
            }
        }

        private void BtnRemoveFromSlider_Click(object sender, RoutedEventArgs e)
        {
            if (LstSlider.SelectedItem is Game g)
            {
                g.IsFeatured = false;
                _gameService.Update(g);
                ServiceLocator.NotifyDataChanged();
                RefreshAll();
            }
        }

        private void BtnAddAnn_Click(object sender, RoutedEventArgs e)
        {
            var ann = new Announcement
            {
                Title = TxtAnnTitle.Text,
                Message = TxtAnnMsg.Text,
                AccentColor = string.IsNullOrWhiteSpace(TxtAnnColor.Text) ? "#9B59FF" : TxtAnnColor.Text,
                IsPinned = ChkAnnPinned.IsChecked == true
            };
            _announcementService.Add(ann);
            _notificationService.NotifyAllUsers(ann.Title, ann.Message, NotificationType.Announcement);
            ServiceLocator.NotifyDataChanged();
            RefreshLists();
        }

        private void BtnUpdateAnn_Click(object sender, RoutedEventArgs e)
        {
            if (LstAnns.SelectedItem is not Announcement a) return;
            a.Title = TxtAnnTitle.Text;
            a.Message = TxtAnnMsg.Text;
            a.AccentColor = string.IsNullOrWhiteSpace(TxtAnnColor.Text) ? "#9B59FF" : TxtAnnColor.Text;
            a.IsPinned = ChkAnnPinned.IsChecked == true;
            _announcementService.Update(a);
            ServiceLocator.NotifyDataChanged();
            RefreshLists();
        }

        private void LoadSelectedAnnouncement()
        {
            if (LstAnns.SelectedItem is not Announcement a) return;
            TxtAnnTitle.Text = a.Title;
            TxtAnnMsg.Text = a.Message;
            TxtAnnColor.Text = a.AccentColor;
            ChkAnnPinned.IsChecked = a.IsPinned;
        }

        private void BtnDeleteAnn_Click(object sender, RoutedEventArgs e)
        {
            if (LstAnns.SelectedItem is Announcement a)
            {
                _announcementService.Remove(a.Id);
                ServiceLocator.NotifyDataChanged();
                RefreshLists();
            }
        }

        private void BtnUserAdd_Click(object sender, RoutedEventArgs e)
        {
            new EditUserWindow(null, _userService).ShowDialog();
            ServiceLocator.NotifyDataChanged();
            RefreshUsers();
        }

        private void BtnUserEdit_Click(object sender, RoutedEventArgs e)
        {
            if (LstUsers.SelectedItem is User u)
            {
                new EditUserWindow(u, _userService).ShowDialog();
                ServiceLocator.NotifyDataChanged();
                RefreshUsers();
            }
        }

        private void BtnUserDelete_Click(object sender, RoutedEventArgs e)
        {
            if (LstUsers.SelectedItem is User u)
            {
                _userService.Delete(u.Id);
                ServiceLocator.NotifyDataChanged();
                RefreshUsers();
            }
        }

        private void BtnGrantVip_Click(object sender, RoutedEventArgs e)
        {
            if (LstUsers.SelectedItem is User u)
            {
                _userService.GrantVip(u.Id, DateTime.UtcNow.AddDays(30));
                u.MembershipLevel = "Gold";
                _userService.Update(u);
                ServiceLocator.NotifyDataChanged();
                RefreshUsers();
            }
        }

        private void BtnRevokeVip_Click(object sender, RoutedEventArgs e)
        {
            if (LstUsers.SelectedItem is User u)
            {
                _userService.RevokeVip(u.Id);
                u.MembershipLevel = "Ücretsiz";
                _userService.Update(u);
                ServiceLocator.NotifyDataChanged();
                RefreshUsers();
            }
        }

        private void BtnExtendVip_Click(object sender, RoutedEventArgs e)
        {
            if (LstUsers.SelectedItem is not User u)
            {
                MessageBox.Show("Lütfen önce bir kullanıcı seçin.", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var dialog = new InputDialog("Kaç gün uzatmak istiyorsunuz?", "30");
            if (dialog.ShowDialog() == true)
            {
                if (int.TryParse(dialog.Result, out int days) && days > 0)
                {
                    var newEnd = (u.VipEndDate ?? DateTime.UtcNow).AddDays(days);
                    _userService.GrantVip(u.Id, newEnd);
                    ServiceLocator.NotifyDataChanged();
                    RefreshUsers();
                    MessageBox.Show($"{u.Username} kullanıcısının VIP süresi {days} gün uzatıldı.\nYeni bitiş tarihi: {newEnd:dd.MM.yyyy}", "Başarılı", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("Geçerli bir gün sayısı girin.", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void BtnMakeAdmin_Click(object sender, RoutedEventArgs e)
        {
            if (LstUsers.SelectedItem is User u)
            {
                _userService.MakeAdmin(u.Id);
                ServiceLocator.NotifyDataChanged();
                RefreshUsers();
            }
        }

        private void BtnRevokeAdmin_Click(object sender, RoutedEventArgs e)
        {
            if (LstUsers.SelectedItem is User u)
            {
                _userService.RevokeAdmin(u.Id);
                ServiceLocator.NotifyDataChanged();
                RefreshUsers();
            }
        }

        private void BtnMakeMod_Click(object sender, RoutedEventArgs e)
        {
            if (LstUsers.SelectedItem is User u)
            {
                _userService.MakeMod(u.Id);
                ServiceLocator.NotifyDataChanged();
                RefreshUsers();
            }
        }

        private void BtnRevokeMod_Click(object sender, RoutedEventArgs e)
        {
            if (LstUsers.SelectedItem is User u)
            {
                _userService.RevokeMod(u.Id);
                ServiceLocator.NotifyDataChanged();
                RefreshUsers();
            }
        }

        private void BtnQuickAddAnnouncement_Click(object sender, RoutedEventArgs e)
        {
            ShowPage("Announcements");
            TxtAnnTitle.Text = "";
            TxtAnnMsg.Text = "";
            TxtAnnColor.Text = "#9B59FF";
            ChkAnnPinned.IsChecked = false;
            TxtAnnTitle.Focus();
        }

        private void BtnQuickSearchUser_Click(object sender, RoutedEventArgs e)
        {
            ShowPage("Users");
            LstUsers.Focus();
        }

        private void BtnQuickGiveVip_Click(object sender, RoutedEventArgs e)
        {
            ShowPage("Users");
            LstUsers.Focus();
        }
    }
}
