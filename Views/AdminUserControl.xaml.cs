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

        public AdminUserControl()
        {
            InitializeComponent();

            _gameService = ServiceLocator.GameService ?? new InMemoryGameService();
            _announcementService = ServiceLocator.AnnouncementService ?? new InMemoryAnnouncementService();
            _userService = ServiceLocator.UserService ?? new InMemoryUserService();
            _settingsService = ServiceLocator.SettingsService ?? new InMemorySettingsService();
            _membershipService = ServiceLocator.MembershipService ?? new InMemoryMembershipService();

            ServiceLocator.DataChanged += OnDataChanged;

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
            BtnDeleteAnn.Click += BtnDeleteAnn_Click;

            BtnUserAdd.Click += BtnUserAdd_Click;
            BtnUserEdit.Click += BtnUserEdit_Click;
            BtnUserDelete.Click += BtnUserDelete_Click;
            BtnGrantVip.Click += BtnGrantVip_Click;
            BtnRevokeVip.Click += BtnRevokeVip_Click;
            BtnExtendVip.Click += BtnExtendVip_Click;
            BtnMakeAdmin.Click += BtnMakeAdmin_Click;
            BtnRevokeAdmin.Click += BtnRevokeAdmin_Click;

            LstMemberships.SelectionChanged += (_, _) => LoadSelectedTier();
            BtnSaveTier.Click += BtnSaveTier_Click;
            BtnDeleteTier.Click += BtnDeleteTier_Click;
            BtnSaveSettings.Click += BtnSaveSettings_Click;

            Unloaded += (_, _) => ServiceLocator.DataChanged -= OnDataChanged;
            RefreshAll();
        }

        private void OnDataChanged() =>
            Dispatcher?.BeginInvoke(new Action(RefreshAll));

        private void RefreshAll()
        {
            RefreshDashboard();
            RefreshLists();
            RefreshUsers();
            RefreshTop10();
            RefreshMemberships();
            LoadSettingsForm();
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
            _announcementService.Add(new Announcement
            {
                Title = TxtAnnTitle.Text,
                Message = TxtAnnMsg.Text,
                AccentColor = string.IsNullOrWhiteSpace(TxtAnnColor.Text) ? "#9B59FF" : TxtAnnColor.Text
            });
            ServiceLocator.NotifyDataChanged();
            RefreshLists();
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
            _userService.Create(new User { Username = "user_" + Guid.NewGuid().ToString()[..6], Email = "" });
            ServiceLocator.NotifyDataChanged();
            RefreshUsers();
        }

        private void BtnUserEdit_Click(object sender, RoutedEventArgs e)
        {
            if (LstUsers.SelectedItem is User u)
            {
                u.Username += "_edit";
                _userService.Update(u);
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
    }
}
