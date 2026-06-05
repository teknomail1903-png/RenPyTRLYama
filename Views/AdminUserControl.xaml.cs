using System;
using System.Windows;
using RenPyTRLauncher.Services;
using RenPyTRLauncher.Models;
using System.Linq;
using System.Windows.Controls;
using System.Collections.ObjectModel;

namespace RenPyTRLauncher.Views
{
    public partial class AdminUserControl : System.Windows.Controls.UserControl
    {
        private readonly IGameService _gameService;
        private readonly IAnnouncementService _announcementService;
        private readonly IUserService _userService;

        public AdminUserControl()
        {
            InitializeComponent();

            _gameService = Services.ServiceLocator.GameService ?? new InMemoryGameService();
            _announcementService = Services.ServiceLocator.AnnouncementService ?? new InMemoryAnnouncementService();
            _userService = Services.ServiceLocator.UserService ?? new InMemoryUserService();
            // Subscribe to data change notifications to keep UI in sync
            Services.ServiceLocator.DataChanged += OnDataChanged;

            // Attach handlers safely (controls may be null during designer/hot-reload)
            var btnTop10Refresh = this.FindName("BtnTop10Refresh") as System.Windows.Controls.Button;
            if (btnTop10Refresh != null) btnTop10Refresh.Click += (s, e) => RefreshTop10();
            var btnTop10Remove = this.FindName("BtnTop10Remove") as System.Windows.Controls.Button;
            if (btnTop10Remove != null) btnTop10Remove.Click += BtnTop10Remove_Click;
            var lstTop = this.FindName("LstTop10") as System.Windows.Controls.ListBox;
            if (lstTop != null) lstTop.MouseDoubleClick += (s, e) => { if (lstTop.SelectedItem is Game gg) { var win = new Views.EditGameWindow(gg, _gameService); win.ShowDialog(); RefreshLists(); RefreshTop10(); } };
            var btnTop10Detail = this.FindName("BtnTop10Detail") as System.Windows.Controls.Button;
            if (btnTop10Detail != null) btnTop10Detail.Click += (s, e) => {
                var lb = GetControl<System.Windows.Controls.ListBox>("LstTop10");
                if (lb?.SelectedItem is Game g) { var win = new Views.EditGameWindow(g, _gameService); win.ShowDialog(); RefreshLists(); RefreshTop10(); }
            };

            var btnAddGame = this.FindName("BtnAddGame") as System.Windows.Controls.Button;
            if (btnAddGame != null) btnAddGame.Click += BtnAddGame_Click;
            var btnDeleteGame = this.FindName("BtnDeleteGame") as System.Windows.Controls.Button;
            if (btnDeleteGame != null) btnDeleteGame.Click += BtnDeleteGame_Click;

            var btnAddAnn = this.FindName("BtnAddAnn") as System.Windows.Controls.Button;
            if (btnAddAnn != null) btnAddAnn.Click += BtnAddAnn_Click;

            var btnEditGame = this.FindName("BtnEditGame") as System.Windows.Controls.Button;
            if (btnEditGame != null) btnEditGame.Click += BtnEditGame_Click;

            var btnUserAdd = this.FindName("BtnUserAdd") as System.Windows.Controls.Button;
            if (btnUserAdd != null) btnUserAdd.Click += BtnUserAdd_Click;
            var btnUserEdit = this.FindName("BtnUserEdit") as System.Windows.Controls.Button;
            if (btnUserEdit != null) btnUserEdit.Click += BtnUserEdit_Click;
            var btnUserDelete = this.FindName("BtnUserDelete") as System.Windows.Controls.Button;
            if (btnUserDelete != null) btnUserDelete.Click += BtnUserDelete_Click;

            var btnGrantVip = this.FindName("BtnGrantVip") as System.Windows.Controls.Button;
            if (btnGrantVip != null) btnGrantVip.Click += BtnGrantVip_Click;
            var btnRevokeVip = this.FindName("BtnRevokeVip") as System.Windows.Controls.Button;
            if (btnRevokeVip != null) btnRevokeVip.Click += BtnRevokeVip_Click;
            var btnExtendVip = this.FindName("BtnExtendVip") as System.Windows.Controls.Button;
            if (btnExtendVip != null) btnExtendVip.Click += BtnExtendVip_Click;

            // Refresh UI once
            RefreshLists();
            RefreshUsers();

            // Unsubscribe when unloaded to avoid leaks
            this.Unloaded += (s, e) => Services.ServiceLocator.DataChanged -= OnDataChanged;
        }

        private T? GetControl<T>(string name) where T : class
        {
            return this.FindName(name) as T;
        }

        private void RefreshLists()
        {
            var lstGames = this.FindName("LstGames") as System.Windows.Controls.ListBox;
            if (lstGames != null)
            {
                lstGames.ItemsSource = null;
                var games = _gameService.GetAll().ToList();
                lstGames.ItemsSource = games;
            }

            var lstAnns = this.FindName("LstAnns") as System.Windows.Controls.ListBox;
            if (lstAnns != null)
            {
                lstAnns.ItemsSource = null;
                lstAnns.ItemsSource = _announcementService.GetAll();
            }
        }

        private void OnDataChanged()
        {
            // Called from ServiceLocator.DataChanged - ensure called on UI thread
            _ = this.Dispatcher?.BeginInvoke(new Action(() =>
            {
                RefreshLists();
                RefreshUsers();
                RefreshTop10();
            }));
        }

        private void RefreshTop10()
        {
            var lstTop = this.FindName("LstTop10") as System.Windows.Controls.ListBox;
            if (lstTop != null)
            {
                lstTop.ItemsSource = null;
                // Use the underlying game service to compute a simple "top 10".
                // Prefer games marked IsTop10 and sort by DownloadCount; fall back to first 10.
                var all = _gameService.GetAll();
                var top = all.Where(g => g.IsTop10).OrderByDescending(g => g.DownloadCount).ToList();
                if (!top.Any())
                    top = all.Take(10).ToList();
                lstTop.ItemsSource = top;
            }
        }

        private void BtnTop10Remove_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var lstTop = this.FindName("LstTop10") as System.Windows.Controls.ListBox;
            if (lstTop?.SelectedItem is Game g)
            {
                // Unmark as top 10 instead of deleting the game
                g.IsTop10 = false;
                _gameService.Update(g);
                Services.ServiceLocator.NotifyDataChanged();
                RefreshLists();
                RefreshTop10();
            }
        }

        private void RefreshUsers()
        {
            LstUsers.ItemsSource = null;
            LstUsers.ItemsSource = _userService.GetAll();
        }

        private void BtnAddGame_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var win = new Views.EditGameWindow(null, _gameService);
            win.ShowDialog();
            RefreshLists();
        }

        private void BtnDeleteGame_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var lst = GetControl<System.Windows.Controls.ListBox>("LstGames");
            if (lst?.SelectedItem is Game g)
            {
                _gameService.Remove(g.Id);
                Services.ServiceLocator.NotifyDataChanged();
                RefreshLists();
                RefreshTop10();
            }
        }

        private void BtnUserAdd_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var u = new Models.User { Username = "yeni_kullanici_" + System.Guid.NewGuid().ToString().Substring(0,4), Email = "" };
            _userService.Create(u);
            Services.ServiceLocator.NotifyDataChanged();
            RefreshUsers();
        }

        private void BtnUserEdit_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var lst = GetControl<System.Windows.Controls.ListBox>("LstUsers");
            if (lst?.SelectedItem is Models.User u)
            {
                u.Username = u.Username + "_edit";
                _userService.Update(u);
                Services.ServiceLocator.NotifyDataChanged();
                RefreshUsers();
            }
        }

        private void BtnUserDelete_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (LstUsers.SelectedItem is Models.User u)
            {
                _userService.Delete(u.Id);
                Services.ServiceLocator.NotifyDataChanged();
                RefreshUsers();
            }
        }

        private void BtnEditGame_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var lst = GetControl<System.Windows.Controls.ListBox>("LstGames");
            if (lst?.SelectedItem is Game g)
            {
                var win = new Views.EditGameWindow(g, _gameService);
                win.ShowDialog();
                RefreshLists();
                RefreshTop10();
            }
        }

        private void BtnGrantVip_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var lst = GetControl<System.Windows.Controls.ListBox>("LstUsers");
            if (lst?.SelectedItem is Models.User u)
            {
                _userService.GrantVip(u.Id, DateTime.UtcNow.AddDays(30));
                Services.ServiceLocator.NotifyDataChanged();
                RefreshUsers();
            }
        }

        private void BtnRevokeVip_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (LstUsers.SelectedItem is Models.User u)
            {
                _userService.RevokeVip(u.Id);
                Services.ServiceLocator.NotifyDataChanged();
                RefreshUsers();
            }
        }

        private void BtnExtendVip_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var lst = GetControl<System.Windows.Controls.ListBox>("LstUsers");
            if (lst?.SelectedItem is Models.User u)
            {
                var newEnd = (u.VipEndDate ?? DateTime.UtcNow).AddDays(30);
                _userService.GrantVip(u.Id, newEnd);
                Services.ServiceLocator.NotifyDataChanged();
                RefreshUsers();
            }
        }

        private void BtnAddAnn_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var titleBox = GetControl<System.Windows.Controls.TextBox>("TxtAnnTitle");
            var msgBox = GetControl<System.Windows.Controls.TextBox>("TxtAnnMsg");
            var a = new Announcement { Title = titleBox?.Text ?? "(başlık)", Message = msgBox?.Text ?? "" };
            _announcementService.Add(a);
            Services.ServiceLocator.NotifyDataChanged();
            RefreshLists();
        }
    }
}
