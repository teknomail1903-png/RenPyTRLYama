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
            InitializeComponent();
            _currentUser = currentUser;

            _gameService = ServiceLocator.GameService ?? new InMemoryGameService();
            _announcementService = ServiceLocator.AnnouncementService ?? new InMemoryAnnouncementService();
            _userService = ServiceLocator.UserService ?? new InMemoryUserService();
            _settingsService = ServiceLocator.SettingsService ?? new InMemorySettingsService();
            _membershipService = ServiceLocator.MembershipService ?? new InMemoryMembershipService();
            _supportService = ServiceLocator.SupportService ?? new InMemorySupportService();
            _categoryService = ServiceLocator.CategoryService ?? new InMemoryCategoryService();
            _helpService = ServiceLocator.HelpService ?? new InMemoryHelpService();
            _notificationService = ServiceLocator.NotificationService ?? new InMemoryNotificationService(_userService);

            ServiceLocator.DataChanged += OnDataChanged;
            ApplyRoleRestrictions();

            BtnTop10Refresh.Click += (_, _) => RefreshTop10();
            BtnTop10Remove.Click += BtnTop10Remove_Click;
            LstTop10.MouseDoubleClick += (_, _) =>
            {
                if (LstTop10.SelectedItem is Game g)
                {
                    new EditGameWindow(g, _gameService).ShowDialog();
                    RefreshAll();
                }
            };

            BtnAddGame.Click += BtnAddGame_Click;
            BtnDeleteGame.Click += BtnDeleteGame_Click;
            BtnEditGame.Click += BtnEditGame_Click;
            BtnAddAnn.Click += BtnAddAnn_Click;
            BtnUpdateAnn.Click += BtnUpdateAnn_Click;
            BtnDeleteAnn.Click += BtnDeleteAnn_Click;
            LstAnns.SelectionChanged += (_, _) => LoadSelectedAnnouncement();

            BtnAddCategory.Click += BtnAddCategory_Click;
            BtnEditCategory.Click += BtnEditCategory_Click;
            BtnDeleteCategory.Click += BtnDeleteCategory_Click;

            BtnAddHelp.Click += BtnAddHelp_Click;
            BtnDeleteHelp.Click += BtnDeleteHelp_Click;

            BtnUserAdd.Click += BtnUserAdd_Click;
            BtnUserEdit.Click += BtnUserEdit_Click;
            BtnUserDelete.Click += BtnUserDelete_Click;
            BtnGrantVip.Click += BtnGrantVip_Click;
            BtnRevokeVip.Click += BtnRevokeVip_Click;
            BtnExtendVip.Click += BtnExtendVip_Click;
            BtnMakeAdmin.Click += BtnMakeAdmin_Click;
            BtnRevokeAdmin.Click += BtnRevokeAdmin_Click;
            BtnMakeMod.Click += BtnMakeMod_Click;
            BtnRevokeMod.Click += BtnRevokeMod_Click;

            LstTickets.SelectionChanged += (_, _) => LoadSelectedTicket();
            BtnReplyTicket.Click += BtnReplyTicket_Click;
            BtnCloseTicket.Click += BtnCloseTicket_Click;
            BtnReopenTicket.Click += BtnReopenTicket_Click;

            LstMemberships.SelectionChanged += (_, _) => LoadSelectedTier();
            BtnSaveTier.Click += BtnSaveTier_Click;
            BtnDeleteTier.Click += BtnDeleteTier_Click;
            BtnSaveSettings.Click += BtnSaveSettings_Click;

            Unloaded += (_, _) => ServiceLocator.DataChanged -= OnDataChanged;
            RefreshAll();
        }

        private void OnDataChanged() =>
            Dispatcher?.BeginInvoke(new Action(RefreshAll));

        private void ApplyRoleRestrictions()
        {
            var isAdmin = AuthorizationService.CanManageGames(_currentUser);
            var isMod = AuthorizationService.CanManageSupport(_currentUser);

            TabGames.Visibility = isAdmin ? Visibility.Visible : Visibility.Collapsed;
            TabUsers.Visibility = isAdmin ? Visibility.Visible : Visibility.Collapsed;
            TabMemberships.Visibility = isAdmin ? Visibility.Visible : Visibility.Collapsed;
            TabTop10.Visibility = isAdmin ? Visibility.Visible : Visibility.Collapsed;
            TabAnnouncements.Visibility = isAdmin ? Visibility.Visible : Visibility.Collapsed;
            TabCategories.Visibility = isAdmin ? Visibility.Visible : Visibility.Collapsed;
            TabHelp.Visibility = isAdmin ? Visibility.Visible : Visibility.Collapsed;
            TabSettings.Visibility = isAdmin ? Visibility.Visible : Visibility.Collapsed;
            TabSupport.Visibility = isMod ? Visibility.Visible : Visibility.Collapsed;
        }

        private void RefreshAll()
        {
            RefreshDashboard();
            RefreshLists();
            RefreshUsers();
            RefreshTop10();
            RefreshMemberships();
            RefreshTickets();
            RefreshCategories();
            RefreshHelpGuides();
            LoadSettingsForm();
        }

        private void RefreshTickets()
        {
            LstTickets.ItemsSource = null;
            LstTickets.ItemsSource = _supportService.GetAll().ToList();
        }

        private void LoadSelectedTicket()
        {
            if (LstTickets.SelectedItem is not SupportTicket t)
            {
                TxtTicketDetail.Text = "";
                TxtTicketReply.Text = "";
                return;
            }
            TxtTicketDetail.Text = $"[{t.TypeLabel}] {t.Subject}\n\n{t.Message}\n\nDurum: {t.StatusLabel}";
            TxtTicketReply.Text = t.AdminReply;
        }

        private void BtnReplyTicket_Click(object sender, RoutedEventArgs e)
        {
            if (LstTickets.SelectedItem is not SupportTicket t || _currentUser == null) return;
            if (string.IsNullOrWhiteSpace(TxtTicketReply.Text)) return;
            _supportService.Reply(t.Id, _currentUser.Id, TxtTicketReply.Text);
            ServiceLocator.NotifyDataChanged();
            RefreshTickets();
        }

        private void BtnCloseTicket_Click(object sender, RoutedEventArgs e)
        {
            if (LstTickets.SelectedItem is SupportTicket t)
            {
                _supportService.Close(t.Id);
                ServiceLocator.NotifyDataChanged();
                RefreshTickets();
            }
        }

        private void BtnReopenTicket_Click(object sender, RoutedEventArgs e)
        {
            if (LstTickets.SelectedItem is SupportTicket t)
            {
                _supportService.Reopen(t.Id);
                ServiceLocator.NotifyDataChanged();
                RefreshTickets();
            }
        }

        private void RefreshDashboard()
        {
            var games = _gameService.GetAll().ToList();
            var users = _userService.GetAll().ToList();
            TxtStatGames.Text = games.Count.ToString();
            TxtStatUsers.Text = users.Count.ToString();
            TxtStatDownloads.Text = games.Sum(g => g.DownloadCount).ToString("N0");
            TxtStatVip.Text = users.Count(u => u.IsVip).ToString();
            TxtStatActive.Text = DownloadTracker.ActiveCount.ToString();
        }

        private void RefreshLists()
        {
            LstGames.ItemsSource = null;
            LstGames.ItemsSource = _gameService.GetAll().ToList();
            LstAnns.ItemsSource = null;
            LstAnns.ItemsSource = _announcementService.GetAll();
        }

        private void RefreshUsers()
        {
            LstUsers.ItemsSource = null;
            LstUsers.ItemsSource = _userService.GetAll();
        }

        private void RefreshTop10()
        {
            LstTop10.ItemsSource = null;
            var all = _gameService.GetAll();
            var top = all.Where(g => g.IsTop10).OrderByDescending(g => g.DownloadCount).ToList();
            if (!top.Any()) top = all.OrderByDescending(g => g.DownloadCount).Take(10).ToList();
            LstTop10.ItemsSource = top;
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
            TxtTierFeatures.Text = string.Join(Environment.NewLine, tier.Features);
        }

        private void BtnSaveTier_Click(object sender, RoutedEventArgs e)
        {
            if (LstMemberships.SelectedItem is MembershipTier tier)
            {
                tier.Name = TxtTierName.Text;
                tier.PriceLabel = TxtTierPrice.Text;
                tier.PurchaseUrl = TxtTierUrl.Text;
                tier.Features = TxtTierFeatures.Text
                    .Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                _membershipService.Update(tier);
            }
            else
            {
                _membershipService.Add(new MembershipTier
                {
                    Name = TxtTierName.Text,
                    PriceLabel = TxtTierPrice.Text,
                    PurchaseUrl = TxtTierUrl.Text,
                    Features = TxtTierFeatures.Text
                        .Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries).ToList(),
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

        private void BtnTop10Remove_Click(object sender, RoutedEventArgs e)
        {
            if (LstTop10.SelectedItem is Game g)
            {
                g.IsTop10 = false;
                _gameService.Update(g);
                ServiceLocator.NotifyDataChanged();
                RefreshAll();
            }
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
                new EditGameWindow(g, _gameService).ShowDialog();
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

        private void RefreshCategories()
        {
            LstCategories.ItemsSource = null;
            LstCategories.ItemsSource = _categoryService.GetAll().ToList();
        }

        private void BtnAddCategory_Click(object sender, RoutedEventArgs e)
        {
            new EditCategoryWindow(null, _categoryService).ShowDialog();
            ServiceLocator.NotifyDataChanged();
            RefreshCategories();
        }

        private void BtnEditCategory_Click(object sender, RoutedEventArgs e)
        {
            if (LstCategories.SelectedItem is GameCategory cat)
            {
                new EditCategoryWindow(cat, _categoryService).ShowDialog();
                ServiceLocator.NotifyDataChanged();
                RefreshCategories();
            }
        }

        private void BtnDeleteCategory_Click(object sender, RoutedEventArgs e)
        {
            if (LstCategories.SelectedItem is GameCategory cat)
            {
                _categoryService.Remove(cat.Id);
                ServiceLocator.NotifyDataChanged();
                RefreshCategories();
            }
        }

        private void RefreshHelpGuides()
        {
            LstHelpGuides.ItemsSource = null;
            LstHelpGuides.ItemsSource = _helpService.GetAll().ToList();
        }

        private void BtnAddHelp_Click(object sender, RoutedEventArgs e)
        {
            var typeTag = (CmbHelpType.SelectedItem as ComboBoxItem)?.Tag?.ToString() ?? "Text";
            var type = Enum.TryParse<HelpGuideType>(typeTag, out var parsed) ? parsed : HelpGuideType.Text;
            _helpService.Add(new HelpGuide
            {
                Title = TxtHelpTitle.Text,
                Content = TxtHelpContent.Text,
                VideoUrl = TxtHelpVideoUrl.Text,
                Type = type,
                SortOrder = _helpService.GetAll().Count() + 1
            });
            ServiceLocator.NotifyDataChanged();
            RefreshHelpGuides();
        }

        private void BtnDeleteHelp_Click(object sender, RoutedEventArgs e)
        {
            if (LstHelpGuides.SelectedItem is HelpGuide g)
            {
                _helpService.Remove(g.Id);
                ServiceLocator.NotifyDataChanged();
                RefreshHelpGuides();
            }
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
            if (LstUsers.SelectedItem is User u)
            {
                var newEnd = (u.VipEndDate ?? DateTime.UtcNow).AddDays(30);
                _userService.GrantVip(u.Id, newEnd);
                ServiceLocator.NotifyDataChanged();
                RefreshUsers();
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
    }
}
