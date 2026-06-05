using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using RenPyTRLauncher.Models;

namespace RenPyTRLauncher.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private System.Windows.Threading.DispatcherTimer? _vipTimer;
        private System.Windows.Threading.DispatcherTimer? _announcementTimer;
        private readonly Services.IUserService _userService;
        private readonly Services.IGameService _gameService;
        private readonly Services.IAnnouncementService _announcementService;
        private readonly Services.ISettingsService _settingsService;
        private readonly Services.IMembershipService _membershipService;
        private readonly Services.IActivityService _activityService;

        private Models.User? _currentUser;
        public Models.User? CurrentUser
        {
            get => _currentUser;
            private set { _currentUser = value; OnPropertyChanged(nameof(CurrentUser)); }
        }

        public ObservableCollection<Game> Games { get; } = new();
        public ObservableCollection<Game> Recent { get; } = new();
        public ObservableCollection<Game> UpdatedGames { get; } = new();
        public ObservableCollection<Game> VipGames { get; } = new();
        public ObservableCollection<LeaderboardEntry> Leaderboard { get; } = new();
        public ObservableCollection<Game> FavoriteGames { get; } = new();
        public ObservableCollection<Game> RecentlyDownloaded { get; } = new();
        public ObservableCollection<Models.Announcement> Announcements { get; } = new();
        public ObservableCollection<MembershipTier> MembershipTiers { get; } = new();
        public ObservableCollection<UserActivity> RecentActivities { get; } = new();
        public ObservableCollection<CategoryFolderItem> CategoryFolders { get; } = new();
        public ObservableCollection<Game> CategoryGames { get; } = new();

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

        public int TotalGames => Games.Count;
        public int TotalUsers => _userService.GetAll().Count();
        public int TotalDownloads => Games.Sum(g => g.DownloadCount);
        public int VipMemberCount => _userService.GetAll().Count(u => u.IsVip);
        public int ActiveDownloads => Services.DownloadTracker.ActiveCount;

        public MainViewModel()
        {
            _userService = Services.ServiceLocator.UserService ?? new Services.InMemoryUserService();
            _gameService = Services.ServiceLocator.GameService ?? new Services.InMemoryGameService();
            _announcementService = Services.ServiceLocator.AnnouncementService ?? new Services.InMemoryAnnouncementService();
            _settingsService = Services.ServiceLocator.SettingsService ?? new Services.InMemorySettingsService();
            _membershipService = Services.ServiceLocator.MembershipService ?? new Services.InMemoryMembershipService();
            _activityService = Services.ServiceLocator.ActivityService ?? new Services.InMemoryActivityService();

            InitCategoryFolders();
            CurrentUser = _userService.GetByUsername("argion");
            Services.ServiceLocator.DataChanged += () => LoadData();

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

        private void InitCategoryFolders()
        {
            CategoryFolders.Clear();
            CategoryFolders.Add(new CategoryFolderItem { DisplayName = "Devam Edenler", CategoryKey = "Devam Eden", Icon = "📁", AccentColor = "#3498DB" });
            CategoryFolders.Add(new CategoryFolderItem { DisplayName = "Bitenler", CategoryKey = "Biten", Icon = "📁", AccentColor = "#27AE60" });
            CategoryFolders.Add(new CategoryFolderItem { DisplayName = "Devam Etmeyenler", CategoryKey = "Devam Etmeyen", Icon = "📁", AccentColor = "#E67E22" });
            CategoryFolders.Add(new CategoryFolderItem { DisplayName = "Erkek Başrol", CategoryKey = "Erkek Başrol", Icon = "📁", AccentColor = "#9B59B6" });
            CategoryFolders.Add(new CategoryFolderItem { DisplayName = "Kadın Başrol", CategoryKey = "Kadın Başrol", Icon = "📁", AccentColor = "#E91E63" });
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

            RefreshSections();
            LoadAnnouncements();
            LoadSettings();
            LoadMembershipTiers();
            RefreshUserProfile();
            RefreshAdminStats();

            if (!string.IsNullOrEmpty(SelectedCategoryFolder))
            {
                var folder = CategoryFolders.FirstOrDefault(f => f.DisplayName == SelectedCategoryFolder);
                if (folder != null) OpenCategoryFolder(folder.CategoryKey, folder.DisplayName);
            }
        }

        private void RefreshCurrentUser()
        {
            if (CurrentUser != null)
            {
                var refreshed = _userService.GetById(CurrentUser.Id);
                if (refreshed != null) CurrentUser = refreshed;
            }
            else
            {
                CurrentUser = _userService.GetByUsername("argion");
            }
        }

        private void LoadAnnouncements()
        {
            Announcements.Clear();
            foreach (var a in _announcementService.GetAll().Where(x => x.IsActive).OrderByDescending(x => x.CreatedAt))
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
            OnPropertyChanged(nameof(WebsiteUrl));
            OnPropertyChanged(nameof(DiscordUrl));
            OnPropertyChanged(nameof(AnnouncementsUrl));
            OnPropertyChanged(nameof(SupportUrl));
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
            VipGames.Clear();
            Leaderboard.Clear();

            foreach (var g in Games.OrderByDescending(g => g.CreatedDate).Take(8))
                Recent.Add(g);

            foreach (var g in Games.OrderByDescending(g => g.UpdatedDate).Take(8))
                UpdatedGames.Add(g);

            foreach (var g in Games.Where(g => g.IsVip))
                VipGames.Add(g);

            var rank = 1;
            foreach (var g in Games.OrderByDescending(g => g.DownloadCount).Take(10))
            {
                Leaderboard.Add(new LeaderboardEntry { Rank = rank++, Game = g });
            }
        }

        public async System.Threading.Tasks.Task<Models.PatchInstallResult> InstallPatchAsync(Models.Game game, string gameRootFolder)
        {
            if (CurrentUser == null)
                return new Models.PatchInstallResult { Success = false, Message = "Giriş yapılmış kullanıcı bulunamadı." };

            var patchService = Services.ServiceLocator.PatchService;
            if (patchService == null)
                return new Models.PatchInstallResult { Success = false, Message = "Yama servisi başlatılamadı." };

            Services.DownloadTracker.Begin();
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
                Services.DownloadTracker.End();
                RefreshAdminStats();
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
