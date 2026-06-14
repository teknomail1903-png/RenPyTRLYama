using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using RenPyTRLauncher.Models;
using RenPyTRLauncher.Services;

namespace RenPyTRLauncher.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private System.Windows.Threading.DispatcherTimer? _vipTimer;
        private System.Windows.Threading.DispatcherTimer? _announcementTimer;
        private readonly IUserService _userService;
        private readonly IGameService _gameService;
        private readonly IAnnouncementService _announcementService;
        private readonly ISettingsService _settingsService;
        private readonly IMembershipService _membershipService;
        private readonly IActivityService _activityService;
        private readonly ISupportService _supportService;
        private readonly IAuthService _authService;
        private readonly ICategoryService _categoryService;
        private readonly INotificationService _notificationService;
        private readonly IHelpService _helpService;

        private Models.User? _currentUser;
        public Models.User? CurrentUser
        {
            get => _currentUser;
            set
            {
                _currentUser = value;
                OnPropertyChanged(nameof(CurrentUser));
                OnPropertyChanged(nameof(IsVipUser));
                OnPropertyChanged(nameof(VipStatusText));
                OnPropertyChanged(nameof(RoleDisplay));
                OnPropertyChanged(nameof(ProfileCityText));
                OnPropertyChanged(nameof(ProfileAgeText));
            }
        }

        public bool IsVipUser => CurrentUser?.IsVip == true;
        public string VipStatusText
        {
            get
            {
                if (CurrentUser == null) return "Giriş yapılmadı";
                if (!CurrentUser.IsVip) return "Ücretsiz Üye";
                var end = CurrentUser.VipEndDate?.ToLocalTime().ToString("dd.MM.yyyy") ?? "Süresiz";
                return $"VIP Aktif — Bitiş: {end}";
            }
        }
        public string RoleDisplay => CurrentUser?.Role switch
        {
            "Admin" => "🛡️ Admin",
            "Mod" => "🔧 Moderatör",
            _ => "👤 Kullanıcı"
        };

        public string ProfileCityText =>
            string.IsNullOrWhiteSpace(CurrentUser?.City) ? "Şehir belirtilmemiş" : CurrentUser!.City;

        public string ProfileAgeText =>
            CurrentUser?.Age.HasValue == true ? $"{CurrentUser.Age} yaşında" : "Yaş belirtilmemiş";

        public int FavoriteCount => CurrentUser?.FavoriteGameIds?.Count ?? 0;
        public int ModCount => FilteredMods.Count;
        public int TotalGamesCount => Games.Count(g => g.Type == GameType.Game);

        public ObservableCollection<Game> Games { get; } = new();
        public ObservableCollection<Game> FilteredGames { get; } = new();
        public ObservableCollection<Game> FilteredMods { get; } = new();
        public ObservableCollection<Game> Recent { get; } = new();
        public ObservableCollection<Game> UpdatedGames { get; } = new();
        public ObservableCollection<Game> CompletedGames { get; } = new();
        public ObservableCollection<Game> VipGames { get; } = new();
        public ObservableCollection<LeaderboardEntry> Leaderboard { get; } = new();
        public ObservableCollection<Game> FavoriteGames { get; } = new();
        public ObservableCollection<Game> RecentlyDownloaded { get; } = new();
        public ObservableCollection<Models.Announcement> Announcements { get; } = new();
        public ObservableCollection<MembershipTier> MembershipTiers { get; } = new();
        public ObservableCollection<UserActivity> RecentActivities { get; } = new();
        public ObservableCollection<CategoryFolderItem> CategoryFolders { get; } = new();
        public ObservableCollection<Game> CategoryGames { get; } = new();
        public ObservableCollection<SupportTicket> SupportTickets { get; } = new();
        public ObservableCollection<Notification> Notifications { get; } = new();
        public ObservableCollection<HelpGuide> TextGuides { get; } = new();
        public ObservableCollection<HelpGuide> VideoGuides { get; } = new();
        public ObservableCollection<HelpGuide> FaqGuides { get; } = new();
        public ObservableCollection<HelpGuide> ToolGuides { get; } = new();
        public ObservableCollection<string> SearchCategories { get; } = new();

        private string _searchText = "";
        public string SearchText
        {
            get => _searchText;
            set
            {
                _searchText = value ?? "";
                OnPropertyChanged(nameof(SearchText));
                ApplySearchFilter();
            }
        }

        private string? _searchCategoryFilter;
        public string? SearchCategoryFilter
        {
            get => _searchCategoryFilter;
            set
            {
                _searchCategoryFilter = value;
                OnPropertyChanged(nameof(SearchCategoryFilter));
                ApplySearchFilter();
            }
        }

        private bool _searchInFavoritesOnly;
        public bool SearchInFavoritesOnly
        {
            get => _searchInFavoritesOnly;
            set
            {
                _searchInFavoritesOnly = value;
                OnPropertyChanged(nameof(SearchInFavoritesOnly));
                ApplySearchFilter();
            }
        }

        private int _unreadNotificationCount;
        public int UnreadNotificationCount
        {
            get => _unreadNotificationCount;
            private set
            {
                if (_unreadNotificationCount == value) return;
                _unreadNotificationCount = value;
                OnPropertyChanged(nameof(UnreadNotificationCount));
                OnPropertyChanged(nameof(HasUnreadNotifications));
            }
        }

        public bool HasUnreadNotifications => UnreadNotificationCount > 0;

        private string? _selectedCategoryFolder;
        public string? SelectedCategoryFolder
        {
            get => _selectedCategoryFolder;
            set
            {
                _selectedCategoryFolder = value;
                OnPropertyChanged(nameof(SelectedCategoryFolder));
                OnPropertyChanged(nameof(IsCategoryFolderView));
                OnPropertyChanged(nameof(IsCategoryGameListView));
            }
        }

        public bool IsCategoryFolderView => string.IsNullOrEmpty(SelectedCategoryFolder);
        public bool IsCategoryGameListView => !string.IsNullOrEmpty(SelectedCategoryFolder);
        public bool IsCategoryGameView => !string.IsNullOrEmpty(SelectedCategoryFolder);
        public int CategoryGameCount => CategoryGames.Count;

        private int _announcementIndex;
        public int AnnouncementIndex
        {
            get => _announcementIndex;
            set
            {
                if (Announcements.Count == 0) return;
                _announcementIndex = ((value % Announcements.Count) + Announcements.Count) % Announcements.Count;
                OnPropertyChanged(nameof(AnnouncementIndex));
                OnPropertyChanged(nameof(CurrentAnnouncement));
                OnPropertyChanged(nameof(AnnouncementCounter));
            }
        }

        public string AnnouncementCounter =>
            Announcements.Count > 0 ? $"{AnnouncementIndex + 1} / {Announcements.Count}" : "0 / 0";

        public Announcement? CurrentAnnouncement =>
            Announcements.Count > 0 && AnnouncementIndex >= 0 && AnnouncementIndex < Announcements.Count
                ? Announcements[AnnouncementIndex]
                : null;

        private int _currentUserTotalDownloads;
        public int CurrentUserTotalDownloads
        {
            get => _currentUserTotalDownloads;
            private set
            {
                if (_currentUserTotalDownloads == value) return;
                _currentUserTotalDownloads = value;
                OnPropertyChanged(nameof(CurrentUserTotalDownloads));
            }
        }

        public string WebsiteUrl { get; private set; } = "https://renpytr.com";
        public string DiscordUrl { get; private set; } = "https://discord.gg/renpytr";
        public string AnnouncementsUrl { get; private set; } = "https://renpytr.com/duyurular";
        public string SupportUrl { get; private set; } = "https://renpytr.com/destek";
        public string CurrentTheme { get; private set; } = ThemeService.SteamDark;

        public int TotalGames => Games.Count;
        public int TotalUsers => _userService.GetAll().Count();
        public int TotalDownloads => Games.Sum(g => g.DownloadCount);
        public int VipMemberCount => _userService.GetAll().Count(u => u.IsVip);
        public int ActiveDownloads => DownloadTracker.ActiveCount;

        public MainViewModel()
        {
            _userService = ServiceLocator.UserService ?? new InMemoryUserService();
            _gameService = ServiceLocator.GameService ?? new InMemoryGameService();
            _announcementService = ServiceLocator.AnnouncementService ?? new InMemoryAnnouncementService();
            _settingsService = ServiceLocator.SettingsService ?? new InMemorySettingsService();
            _membershipService = ServiceLocator.MembershipService ?? new InMemoryMembershipService();
            _activityService = ServiceLocator.ActivityService ?? new InMemoryActivityService();
            _supportService = ServiceLocator.SupportService ?? new InMemorySupportService();
            _authService = ServiceLocator.AuthService ?? new AuthService(_userService);
            _categoryService = ServiceLocator.CategoryService ?? new InMemoryCategoryService();
            _notificationService = ServiceLocator.NotificationService ?? new InMemoryNotificationService(_userService);
            _helpService = ServiceLocator.HelpService ?? new InMemoryHelpService();

            App.Log("[MAINVIEWMODEL] Constructor - ServiceLocator.UserService is null: " + (ServiceLocator.UserService == null));
            App.Log("[MAINVIEWMODEL] Constructor - Using in-memory fallback: " + (_userService is InMemoryUserService));

            ThemeService.LoadFromSettings(_settingsService);
            CurrentTheme = ThemeService.CurrentTheme;

            LoadCategoryFolders();
            CurrentUser = _authService.CurrentUser ?? _authService.TryRestoreSession();
            App.Log("[MAINVIEWMODEL] Constructor - CurrentUser: " + (CurrentUser?.Username ?? "null") + ", Role: " + (CurrentUser?.Role ?? "null"));
            ServiceLocator.DataChanged += () => LoadData();

            LoadData();

            try
            {
                _vipTimer = new System.Windows.Threading.DispatcherTimer { Interval = TimeSpan.FromHours(1) };
                _vipTimer.Tick += (_, _) => CheckVipExpiry();
                _vipTimer.Start();

                _announcementTimer = new System.Windows.Threading.DispatcherTimer { Interval = TimeSpan.FromSeconds(6) };
                _announcementTimer.Tick += (_, _) => NextAnnouncement();
                _announcementTimer.Start();
            }
            catch { }
        }

        public void LoadCategoryFolders()
        {
            CategoryFolders.Clear();
            foreach (var cat in _categoryService.GetActive())
            {
                CategoryFolders.Add(new CategoryFolderItem
                {
                    DisplayName = cat.DisplayName,
                    CategoryKey = cat.Name,
                    Icon = cat.Icon,
                    AccentColor = cat.AccentColor
                });
            }

            SearchCategories.Clear();
            SearchCategories.Add("Tüm Kategoriler");
            foreach (var cat in _categoryService.GetActive())
                SearchCategories.Add(cat.Name);
        }

        public void OpenCategoryFolder(string categoryKey, string displayName)
        {
            SelectedCategoryFolder = displayName;
            CategoryGames.Clear();
            foreach (var g in Games.Where(g => g.Categories.Any(c =>
                string.Equals(c, categoryKey, StringComparison.OrdinalIgnoreCase))))
                CategoryGames.Add(g);
            OnPropertyChanged(nameof(CategoryGameCount));
        }

        public void CloseCategoryFolder()
        {
            SelectedCategoryFolder = null;
            CategoryGames.Clear();
            OnPropertyChanged(nameof(CategoryGameCount));
        }

        public void NextAnnouncement() => AnnouncementIndex = AnnouncementIndex + 1;
        public void PrevAnnouncement() => AnnouncementIndex = AnnouncementIndex - 1;

        private void LoadData()
        {
            CheckVipExpiry();
            RefreshCurrentUser();

            Games.Clear();
            foreach (var g in _gameService.GetAll()) Games.Add(g);

            LoadCategoryFolders();
            ApplySearchFilter();
            RefreshSections();
            LoadAnnouncements();
            LoadSettings();
            LoadMembershipTiers();
            RefreshUserProfile();
            RefreshAdminStats();
            RefreshSupportTickets();
            RefreshNotifications();
            LoadHelpGuides();

            if (!string.IsNullOrEmpty(SelectedCategoryFolder))
            {
                var folder = CategoryFolders.FirstOrDefault(f => f.DisplayName == SelectedCategoryFolder);
                if (folder != null) OpenCategoryFolder(folder.CategoryKey, folder.DisplayName);
            }
        }

        public void ApplySearchFilter()
        {
            FilteredGames.Clear();
            FilteredMods.Clear();

            var query = Games.AsEnumerable();

            if (SearchInFavoritesOnly && CurrentUser != null)
            {
                var favIds = CurrentUser.FavoriteGameIds.ToHashSet();
                query = query.Where(g => favIds.Contains(g.Id));
            }

            if (!string.IsNullOrWhiteSpace(SearchCategoryFilter) &&
                SearchCategoryFilter != "Tüm Kategoriler")
            {
                query = query.Where(g => g.Categories.Any(c =>
                    string.Equals(c, SearchCategoryFilter, StringComparison.OrdinalIgnoreCase)));
            }

            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                var term = SearchText.Trim();
                query = query.Where(g =>
                    g.Name.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                    g.Description.Contains(term, StringComparison.OrdinalIgnoreCase));
            }

            foreach (var g in query)
            {
                if (g.Type == GameType.Game)
                    FilteredGames.Add(g);
                else if (g.Type == GameType.Translation ||
                         g.Type == GameType.Gallery ||
                         g.Type == GameType.Cheat ||
                         g.Type == GameType.Walkthrough ||
                         g.Type == GameType.Save ||
                         g.Type == GameType.Extra)
                    FilteredMods.Add(g);
            }

            OnPropertyChanged(nameof(SearchResultCount));
            OnPropertyChanged(nameof(ModCount));
        }

        public int SearchResultCount => FilteredGames.Count;

        public bool IsFavorite(Guid gameId) =>
            CurrentUser?.FavoriteGameIds.Contains(gameId) == true;

        public void ToggleFavorite(Game game)
        {
            if (CurrentUser == null) return;
            var wasFavorite = IsFavorite(game.Id);
            _userService.ToggleFavorite(CurrentUser.Id, game.Id);
            RefreshCurrentUser();
            RefreshUserProfile();
            _activityService.Log(CurrentUser.Id,
                wasFavorite ? $"{game.Name} favorilerden çıkarıldı" : $"{game.Name} favorilere eklendi",
                wasFavorite ? "💔" : "⭐");
            OnPropertyChanged(nameof(FavoriteCount));
            ApplySearchFilter();
        }

        public void RefreshNotifications()
        {
            Notifications.Clear();
            if (CurrentUser == null)
            {
                UnreadNotificationCount = 0;
                return;
            }
            foreach (var n in _notificationService.GetForUser(CurrentUser.Id))
                Notifications.Add(n);
            UnreadNotificationCount = _notificationService.GetUnreadCount(CurrentUser.Id);
        }

        public void MarkNotificationRead(Notification notification)
        {
            _notificationService.MarkAsRead(notification.Id);
            notification.IsRead = true;
            RefreshNotifications();
        }

        public void MarkAllNotificationsRead()
        {
            if (CurrentUser == null) return;
            _notificationService.MarkAllAsRead(CurrentUser.Id);
            RefreshNotifications();
        }

        private void LoadHelpGuides()
        {
            TextGuides.Clear();
            VideoGuides.Clear();
            FaqGuides.Clear();
            ToolGuides.Clear();

            foreach (var g in _helpService.GetByType(HelpGuideType.Text)) TextGuides.Add(g);
            foreach (var g in _helpService.GetByType(HelpGuideType.Video)) VideoGuides.Add(g);
            foreach (var g in _helpService.GetByType(HelpGuideType.FAQ)) FaqGuides.Add(g);
            foreach (var g in _helpService.GetByType(HelpGuideType.Tool)) ToolGuides.Add(g);
        }

        public void SetTheme(string theme)
        {
            ThemeService.SaveTheme(_settingsService, theme);
            CurrentTheme = theme;
            OnPropertyChanged(nameof(CurrentTheme));
        }

        private void RefreshCurrentUser()
        {
            if (CurrentUser != null)
            {
                var refreshed = _userService.GetById(CurrentUser.Id);
                if (refreshed != null)
                {
                    AuthorizationService.SyncVipBadges(refreshed);
                    CurrentUser = refreshed;
                }
            }
            else
            {
                CurrentUser = _authService.CurrentUser ?? _authService.TryRestoreSession();
            }
        }

        public void RefreshSupportTickets()
        {
            SupportTickets.Clear();
            if (CurrentUser == null) return;
            foreach (var t in _supportService.GetForUser(CurrentUser.Id))
                SupportTickets.Add(t);
        }

        public void CreateSupportTicket(SupportTicketType type, string subject, string message)
        {
            if (CurrentUser == null)
            {
                System.Windows.MessageBox.Show("Destek talebi için giriş yapmalısınız.", "Destek",
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                return;
            }
            if (string.IsNullOrWhiteSpace(subject) || string.IsNullOrWhiteSpace(message))
            {
                System.Windows.MessageBox.Show("Konu ve mesaj alanları zorunludur.", "Destek",
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                return;
            }

            var ticket = new SupportTicket
            {
                UserId = CurrentUser.Id,
                Subject = subject.Trim(),
                Message = message.Trim(),
                Type = type
            };
            _supportService.Create(ticket);
            _activityService.Log(CurrentUser.Id, $"{ticket.TypeLabel} oluşturuldu: {subject}", "🆘");
            RefreshSupportTickets();
            TicketFormCleared?.Invoke();
            System.Windows.MessageBox.Show("Talebiniz başarıyla gönderildi.", "Destek",
                System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
        }

        public event Action? TicketFormCleared;

        private void LoadAnnouncements()
        {
            Announcements.Clear();
            var all = _announcementService.GetAll().Where(x => x.IsActive).ToList();
            foreach (var a in all.Where(x => x.IsPinned).OrderByDescending(x => x.CreatedAt))
                Announcements.Add(a);
            foreach (var a in all.Where(x => !x.IsPinned).OrderByDescending(x => x.CreatedAt))
                Announcements.Add(a);
            AnnouncementIndex = 0;
            OnPropertyChanged(nameof(AnnouncementCounter));
        }

        private void LoadSettings()
        {
            WebsiteUrl = _settingsService.Get(AppSettingKeys.WebsiteUrl, WebsiteUrl);
            DiscordUrl = _settingsService.Get(AppSettingKeys.DiscordUrl, DiscordUrl);
            AnnouncementsUrl = _settingsService.Get(AppSettingKeys.AnnouncementsUrl, AnnouncementsUrl);
            SupportUrl = _settingsService.Get(AppSettingKeys.SupportUrl, SupportUrl);
            CurrentTheme = _settingsService.Get(AppSettingKeys.Theme, ThemeService.SteamDark);
            OnPropertyChanged(nameof(WebsiteUrl));
            OnPropertyChanged(nameof(DiscordUrl));
            OnPropertyChanged(nameof(AnnouncementsUrl));
            OnPropertyChanged(nameof(SupportUrl));
            OnPropertyChanged(nameof(CurrentTheme));
        }

        private void LoadMembershipTiers()
        {
            MembershipTiers.Clear();
            foreach (var t in _membershipService.GetAll()) MembershipTiers.Add(t);
        }

        private void RefreshUserProfile()
        {
            FavoriteGames.Clear();
            RecentlyDownloaded.Clear();
            RecentActivities.Clear();

            if (CurrentUser == null)
            {
                CurrentUserTotalDownloads = 0;
                OnPropertyChanged(nameof(FavoriteCount));
                return;
            }

            var gameLookup = Games.ToDictionary(g => g.Id);

            foreach (var id in CurrentUser.FavoriteGameIds)
                if (gameLookup.TryGetValue(id, out var game)) FavoriteGames.Add(game);

            foreach (var id in CurrentUser.RecentDownloadedGameIds)
                if (gameLookup.TryGetValue(id, out var game)) RecentlyDownloaded.Add(game);

            foreach (var act in _activityService.GetForUser(CurrentUser.Id))
                RecentActivities.Add(act);

            CurrentUserTotalDownloads = CurrentUser.TotalDownloadCount;
            OnPropertyChanged(nameof(FavoriteCount));
            OnPropertyChanged(nameof(ProfileCityText));
            OnPropertyChanged(nameof(ProfileAgeText));
        }

        public void RefreshAdminStats()
        {
            OnPropertyChanged(nameof(TotalGames));
            OnPropertyChanged(nameof(TotalUsers));
            OnPropertyChanged(nameof(TotalDownloads));
            OnPropertyChanged(nameof(VipMemberCount));
            OnPropertyChanged(nameof(ActiveDownloads));
        }

        public void CheckVipExpiry()
        {
            foreach (var u in _userService.GetAll())
            {
                if (u.IsVip && u.VipEndDate.HasValue && u.VipEndDate.Value < DateTime.UtcNow)
                {
                    _userService.RevokeVip(u.Id);
                    if (CurrentUser?.Id == u.Id)
                    {
                        CurrentUser.IsVip = false;
                        CurrentUser.VipEndDate = null;
                        CurrentUser.MembershipLevel = "Ücretsiz";
                        OnPropertyChanged(nameof(CurrentUser));
                    }
                }
            }
        }

        private void RefreshSections()
        {
            Recent.Clear();
            UpdatedGames.Clear();
            CompletedGames.Clear();
            VipGames.Clear();
            Leaderboard.Clear();

            foreach (var g in Games.OrderByDescending(g => g.CreatedDate).Take(8))
                Recent.Add(g);

            foreach (var g in Games.OrderByDescending(g => g.UpdatedDate).Take(8))
                UpdatedGames.Add(g);

            foreach (var g in Games.OrderByDescending(g => g.UpdatedDate).Where(g => g.Version != "1.0" && g.PatchFilePath != "").Take(8))
                CompletedGames.Add(g);

            foreach (var g in Games.Where(g => g.IsVip))
                VipGames.Add(g);

            var rank = 1;
            foreach (var g in Games.OrderByDescending(g => g.DownloadCount).Take(10))
                Leaderboard.Add(new LeaderboardEntry { Rank = rank++, Game = g });
        }

        public async System.Threading.Tasks.Task<PatchInstallResult> InstallPatchAsync(Game game, string gameRootFolder)
        {
            if (CurrentUser == null)
                return new PatchInstallResult { Success = false, Message = "Giriş yapılmış kullanıcı bulunamadı." };

            var patchService = ServiceLocator.PatchService;
            if (patchService == null)
                return new PatchInstallResult { Success = false, Message = "Yama servisi başlatılamadı." };

            DownloadTracker.Begin();
            try
            {
                var result = await patchService.InstallPatchAsync(game, gameRootFolder, CurrentUser);
                if (result.Success)
                {
                    _activityService.Log(CurrentUser.Id, $"{game.Name} yaması kuruldu", "🔧");
                    LoadData();
                }
                return result;
            }
            finally
            {
                DownloadTracker.End();
                RefreshAdminStats();
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
