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
        private readonly Services.IUserService _userService;
        private readonly Services.IGameService _gameService;
        private readonly Services.IAnnouncementService _announcementService;

        private Models.User? _currentUser;
        public Models.User? CurrentUser
        {
            get => _currentUser;
            private set { _currentUser = value; OnPropertyChanged(nameof(CurrentUser)); }
        }

        public ObservableCollection<Game> Games { get; } = new();
        private ObservableCollection<Game> _filtered = new();
        public ObservableCollection<Game> FilteredGames
        {
            get => _filtered;
            set { _filtered = value; OnPropertyChanged(nameof(FilteredGames)); }
        }

        public ObservableCollection<Game> Featured { get; } = new();
        public ObservableCollection<Game> Recent { get; } = new();
        public ObservableCollection<Game> Top10 { get; } = new();
        public ObservableCollection<Game> VipGames { get; } = new();
        // Profil ile ilgili koleksiyonlar
        public ObservableCollection<Game> FavoriteGames { get; } = new();
        public ObservableCollection<Game> DownloadedPatches { get; } = new();
        public ObservableCollection<Game> RecentlyDownloaded { get; } = new();
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
        public ObservableCollection<Models.Announcement> Announcements { get; } = new();

        // Kategori filtreleme destekleri
        public ObservableCollection<string> Categories { get; } = new();
        private System.Collections.Generic.HashSet<string> _selectedCategories = new();
        public System.Collections.Generic.IEnumerable<string> SelectedCategories => _selectedCategories;
        private string _searchText = string.Empty;
        public string SearchText { get => _searchText; set { _searchText = value ?? string.Empty; OnPropertyChanged(nameof(SearchText)); } }

        public void ToggleCategorySelection(string category)
        {
            if (string.IsNullOrWhiteSpace(category)) return;
            if (_selectedCategories.Contains(category)) _selectedCategories.Remove(category);
            else _selectedCategories.Add(category);
            ApplyFilters();
            OnPropertyChanged(nameof(SelectedCategories));
        }

        public void ClearCategorySelection()
        {
            _selectedCategories.Clear();
            ApplyFilters();
            OnPropertyChanged(nameof(SelectedCategories));
        }

        public void ApplyFilters()
        {
            var query = Games.AsEnumerable();
            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                var s = SearchText.ToLowerInvariant();
                query = query.Where(g => g.Name.ToLowerInvariant().Contains(s) || (g.Description ?? string.Empty).ToLowerInvariant().Contains(s));
            }

            if (_selectedCategories.Any())
            {
                query = query.Where(g => g.Categories.Any(c => _selectedCategories.Contains(c)));
            }

            FilteredGames = new ObservableCollection<Game>(query);
        }

        public MainViewModel()
        {
            // use shared services if available
            _userService = Services.ServiceLocator.UserService ?? new Services.InMemoryUserService();
            _gameService = Services.ServiceLocator.GameService ?? new Services.InMemoryGameService();
            _announcementService = Services.ServiceLocator.AnnouncementService ?? new Services.InMemoryAnnouncementService();

            // örnek current user
            CurrentUser = _userService.GetByUsername("argion");

            // subscribe to data change notifications
            Services.ServiceLocator.DataChanged += () => LoadData();

            // load initial data
            LoadData();

            // start periodic VIP expiry check every hour
            try
            {
                _vipTimer = new System.Windows.Threading.DispatcherTimer();
                _vipTimer.Interval = TimeSpan.FromHours(1);
                _vipTimer.Tick += (s, e) => CheckVipExpiry();
                _vipTimer.Start();
            }
            catch { /* if dispatcher not available, ignore - safe fallback */ }
        }

        private void LoadData()
        {
            CheckVipExpiry();

            if (CurrentUser != null)
            {
                var refreshed = _userService.GetById(CurrentUser.Id);
                if (refreshed != null) CurrentUser = refreshed;
            }
            else
            {
                CurrentUser = _userService.GetByUsername("argion");
            }

            Games.Clear();
            foreach (var g in _gameService.GetAll()) Games.Add(g);

            RefreshSections();
            Announcements.Clear();
            foreach (var a in _announcementService.GetAll()) Announcements.Add(a);

            FilteredGames = new ObservableCollection<Game>(Games);

            // kategorileri doldur
            Categories.Clear();
            var cats = Games.SelectMany(g => g.Categories).Where(c => !string.IsNullOrWhiteSpace(c)).Select(c => c.Trim()).Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(c => c);
            foreach (var c in cats) Categories.Add(c);

            RefreshUserProfile();
        }

        private void RefreshUserProfile()
        {
            FavoriteGames.Clear();
            DownloadedPatches.Clear();
            RecentlyDownloaded.Clear();

            if (CurrentUser == null)
            {
                CurrentUserTotalDownloads = 0;
                return;
            }

            var gameLookup = Games.ToDictionary(g => g.Id);

            foreach (var id in CurrentUser.FavoriteGameIds)
            {
                if (gameLookup.TryGetValue(id, out var game))
                    FavoriteGames.Add(game);
            }

            foreach (var id in CurrentUser.DownloadedPatchIds)
            {
                if (gameLookup.TryGetValue(id, out var game))
                    DownloadedPatches.Add(game);
            }

            foreach (var id in CurrentUser.RecentDownloadedGameIds)
            {
                if (gameLookup.TryGetValue(id, out var game))
                    RecentlyDownloaded.Add(game);
            }

            CurrentUserTotalDownloads = CurrentUser.TotalDownloadCount;
        }

        public void CheckVipExpiry()
        {
            var users = _userService.GetAll();
            foreach (var u in users)
            {
                if (u.IsVip && u.VipEndDate.HasValue && u.VipEndDate.Value < DateTime.UtcNow)
                {
                    _userService.RevokeVip(u.Id);
                    if (CurrentUser != null && CurrentUser.Id == u.Id)
                    {
                        CurrentUser.IsVip = false;
                        CurrentUser.VipEndDate = null;
                        OnPropertyChanged(nameof(CurrentUser));
                    }
                }
            }
        }

        private void RefreshSections()
        {
            Featured.Clear();
            Recent.Clear();
            Top10.Clear();
            VipGames.Clear();

            foreach (var g in Games)
            {
                if (g.IsFeatured) Featured.Add(g);
                if (g.IsTop10) Top10.Add(g);
                if (g.IsVip) VipGames.Add(g);
            }

            foreach (var r in Games.OrderByDescending(g => g.CreatedDate).Take(6)) Recent.Add(r);
        }

        public void FilterByCategory(string category)
        {
            if (string.IsNullOrWhiteSpace(category) || category == "Tüm Oyunlar")
            {
                FilteredGames = new ObservableCollection<Game>(Games);
                return;
            }

            // launcher-odakli filtreler
            if (category.Equals("VIP", StringComparison.OrdinalIgnoreCase))
            {
                FilteredGames = new ObservableCollection<Game>(Games.Where(g => g.IsVip));
                return;
            }

            if (category.Equals("Devam Eden", StringComparison.OrdinalIgnoreCase))
            {
                FilteredGames = new ObservableCollection<Game>(Games.Where(g => g.Categories.Contains("Devam Eden")));
                return;
            }

            if (category.Equals("Biten", StringComparison.OrdinalIgnoreCase))
            {
                FilteredGames = new ObservableCollection<Game>(Games.Where(g => g.Categories.Contains("Biten")));
                return;
            }

            var filtered = Games.Where(g => g.Categories.Contains(category)).ToList();
            FilteredGames = new ObservableCollection<Game>(filtered);
        }

        public async System.Threading.Tasks.Task<Models.PatchInstallResult> InstallPatchAsync(Models.Game game, string gameRootFolder)
        {
            if (CurrentUser == null)
                return new Models.PatchInstallResult { Success = false, Message = "Giriş yapılmış kullanıcı bulunamadı." };

            var patchService = Services.ServiceLocator.PatchService;
            if (patchService == null)
                return new Models.PatchInstallResult { Success = false, Message = "Yama servisi başlatılamadı." };

            var result = await patchService.InstallPatchAsync(game, gameRootFolder, CurrentUser);
            if (result.Success)
                LoadData();
            return result;
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
