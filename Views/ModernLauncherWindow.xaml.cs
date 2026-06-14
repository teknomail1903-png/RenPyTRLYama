using System.Windows;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using RenPyTRLauncher.Services;
using RenPyTRLauncher.Data;
using RenPyTRLauncher.Models;
using RenPyTRLauncher.Config;
using System.Collections.Generic;
using System.Linq;

namespace RenPyTRLauncher.Views
{
    public partial class ModernLauncherWindow : Window
    {
        private readonly IAuthService? _auth;
        private readonly IGameService? _gameService;
        private DispatcherTimer _bannerTimer;
        private int _currentBannerIndex = 0;
        private readonly List<BannerItem> _banners = new List<BannerItem>
        {
            new BannerItem { Title = "RenPy TR Launcher", Subtitle = "En iyi Türkçe oyunlar burada", Color = "#6C5CE7" },
            new BannerItem { Title = "VIP Üyelik", Subtitle = "Özel içeriklere erişim", Color = "#FFD700" },
            new BannerItem { Title = "Yeni Oyunlar", Subtitle = "Her hafta güncelleniyor", Color = "#00CEC9" }
        };

        public ModernLauncherWindow()
        {
            InitializeComponent();
            
            try
            {
                _auth = ServiceLocator.AuthService ?? new AuthService(
                    ServiceLocator.UserService ?? new InMemoryUserService());
                _gameService = ServiceLocator.GameService;
            }
            catch
            {
                // Ignore errors for now
            }

            // Enable window dragging
            MouseLeftButtonDown += (s, e) => DragMove();

            // Load configuration
            LauncherConfig.LoadConfig();

            // Initialize banner carousel
            InitializeBannerCarousel();

            // Load popular games
            LoadPopularGames();

            // Load news
            LoadNews();
        }

        private void InitializeBannerCarousel()
        {
            _bannerTimer = new DispatcherTimer();
            _bannerTimer.Interval = System.TimeSpan.FromSeconds(5);
            _bannerTimer.Tick += BannerTimer_Tick;
            _bannerTimer.Start();
            LoadBannerImages();
            UpdateBanner();
        }

        private void LoadBannerImages()
        {
            if (LauncherConfig.UseRealBannerImages && _gameService != null)
            {
                try
                {
                    var featuredGames = _gameService.GetAll()
                        .Where(g => g.IsFeatured)
                        .Take(3)
                        .ToList();

                    if (featuredGames.Count > 0)
                    {
                        _banners.Clear();
                        foreach (var game in featuredGames)
                        {
                            _banners.Add(new BannerItem 
                            { 
                                Title = game.Name, 
                                Subtitle = $"{game.Description.Substring(0, Math.Min(50, game.Description.Length))}...", 
                                Color = "#6C5CE7",
                                Game = game
                            });
                        }
                    }
                }
                catch
                {
                    // Use default banners if loading fails
                }
            }
        }

        private void BannerTimer_Tick(object sender, System.EventArgs e)
        {
            _currentBannerIndex = (_currentBannerIndex + 1) % _banners.Count;
            UpdateBanner();
        }

        private void UpdateBanner()
        {
            var banner = _banners[_currentBannerIndex];
            BannerTitle.Text = banner.Title;
            BannerSubtitle.Text = banner.Subtitle;

            // Load banner image if game is available
            if (banner.Game != null && !string.IsNullOrEmpty(banner.Game.ImagePath))
            {
                try
                {
                    var imagePath = ImageService.ResolvePath(banner.Game.ImagePath);
                    if (!string.IsNullOrEmpty(imagePath))
                    {
                        BannerImage.Source = new System.Windows.Media.Imaging.BitmapImage(new Uri(imagePath, UriKind.RelativeOrAbsolute));
                    }
                }
                catch
                {
                    BannerImage.Source = null;
                }
            }

            // Update indicators
            IndicatorDot0.Fill = _currentBannerIndex == 0 ? System.Windows.Media.Brushes.Purple : new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(61, 70, 89));
            IndicatorDot1.Fill = _currentBannerIndex == 1 ? System.Windows.Media.Brushes.Purple : new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(61, 70, 89));
            IndicatorDot2.Fill = _currentBannerIndex == 2 ? System.Windows.Media.Brushes.Purple : new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(61, 70, 89));
        }

        private void LoadPopularGames()
        {
            try
            {
                if (_gameService != null)
                {
                    var allGames = _gameService.GetAll();
                    var popularGames = allGames
                        .OrderByDescending(g => g.DownloadCount)
                        .Take(5)
                        .Select(g => new GameCardItem
                        {
                            Name = g.Name,
                            ImagePath = g.ImagePath,
                            TurkishStatusLabel = GetTurkishStatusLabel(g.TurkishStatus),
                            TurkishStatusColor = GetTurkishStatusColor(g.TurkishStatus),
                            Game = g
                        })
                        .ToList();

                    PopularGamesList.ItemsSource = popularGames;
                }
            }
            catch
            {
                // Ignore errors for now
            }
        }

        private string GetTurkishStatusLabel(TurkishStatus status)
        {
            return status switch
            {
                TurkishStatus.Evet => "🇹🇷 %100",
                TurkishStatus.KismiCeviri => "🇹🇷 %75",
                TurkishStatus.DevamEdiyor => "🇹🇷 %50",
                TurkishStatus.Hayır => "🇬🇧 %0",
                _ => "🇹🇷 %0"
            };
        }

        private System.Windows.Media.Brush GetTurkishStatusColor(TurkishStatus status)
        {
            return status switch
            {
                TurkishStatus.Evet => new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(16, 185, 129)),
                TurkishStatus.KismiCeviri => new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(59, 130, 246)),
                TurkishStatus.DevamEdiyor => new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(245, 158, 11)),
                TurkishStatus.Hayır => new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(239, 68, 68)),
                _ => new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(107, 114, 128))
            };
        }

        private void GameCard_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (sender is System.Windows.Controls.Border border && border.DataContext is GameCardItem item && item.Game != null)
            {
                // Show message that user needs to login first
                MessageBox.Show($"{item.Name} detaylarını görüntülemek için önce giriş yapmalısınız.", "Giriş Gerekli", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void BtnVIP_Click(object sender, RoutedEventArgs e)
        {
            // Open VIP market link from config
            try
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo 
                { 
                    FileName = LauncherConfig.VipMarketUrl, 
                    UseShellExecute = true 
                });
            }
            catch
            {
                MessageBox.Show("VIP market sayfası açılamadı.", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnDiscord_Click(object sender, RoutedEventArgs e)
        {
            // Open Discord link from config
            try
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo 
                { 
                    FileName = LauncherConfig.DiscordUrl, 
                    UseShellExecute = true 
                });
            }
            catch
            {
                MessageBox.Show("Discord bağlantısı açılamadı.", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadNews()
        {
            TxtNews.Text = LauncherConfig.NewsText;
        }

        private void BtnMinimize_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void BtnMaximize_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            App.Current.Shutdown();
        }

        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            App.Log("[LOGIN] Button clicked");

            try
            {
                App.Log("[LOGIN] Checking _auth null status");
                if (_auth == null)
                {
                    App.Log("[LOGIN] ERROR: _auth is NULL");
                    MessageBox.Show("Authentication service is not initialized.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                App.Log("[LOGIN] AuthService NULL: False");

                App.Log("[LOGIN] Checking controls null status");
                if (TxtUsername == null || TxtPassword == null)
                {
                    App.Log("[LOGIN] ERROR: TxtUsername or TxtPassword is NULL");
                    MessageBox.Show("Login controls not found.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                App.Log("[LOGIN] Reading username");
                var username = TxtUsername.Text?.Trim();
                App.Log($"[LOGIN] Username: {username ?? "(null)"}");

                App.Log("[LOGIN] Reading password");
                var password = GetPassword(TxtPassword, TxtPasswordVisible);
                App.Log($"[LOGIN] Password length: {password?.Length ?? 0}");

                var remember = ChkRemember?.IsChecked == true;
                App.Log($"[LOGIN] Remember me: {remember}");

                if (string.IsNullOrEmpty(username))
                {
                    App.Log("[LOGIN] ERROR: Username is empty");
                    MessageBox.Show("Kullanıcı adı boş olamaz.", "Giriş Hatası", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (string.IsNullOrEmpty(password))
                {
                    App.Log("[LOGIN] ERROR: Password is empty");
                    MessageBox.Show("Şifre boş olamaz.", "Giriş Hatası", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                App.Log("[LOGIN] Calling Authenticate");
                var result = _auth.Login(username, password, remember);
                App.Log($"[LOGIN] Authenticate result - Success: {result.Success}, Message: {result.Message}");

                if (!result.Success)
                {
                    App.Log("[LOGIN] ERROR: Login failed");
                    MessageBox.Show(result.Message, "Giriş Hatası", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                App.Log("[LOGIN] Login successful, opening MainWindow");
                // Open main window
                var main = new MainWindow();
                main.Show();
                Close();
                App.Log("[LOGIN] MainWindow opened, login window closed");
            }
            catch (Exception ex)
            {
                App.Log($"[LOGIN] EXCEPTION: {ex.Message}\n{ex.StackTrace}");
                MessageBox.Show($"Login error: {ex.Message}\n\n{ex.StackTrace}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnRegister_Click(object sender, RoutedEventArgs e)
        {
            var registerWindow = new RegisterWindow();
            registerWindow.ShowDialog();
        }

        private void BtnForgotPassword_Click(object sender, RoutedEventArgs e)
        {
            var forgotPasswordWindow = new ForgotPasswordWindow();
            forgotPasswordWindow.ShowDialog();
        }

        private void BtnTogglePassword_Click(object sender, RoutedEventArgs e)
        {
            TogglePasswordVisibility(TxtPassword, TxtPasswordVisible, BtnTogglePassword);
        }

        private void TogglePasswordVisibility(System.Windows.Controls.PasswordBox passwordBox, System.Windows.Controls.TextBox textBox, System.Windows.Controls.Button toggleButton)
        {
            if (passwordBox.Visibility == Visibility.Visible)
            {
                textBox.Text = passwordBox.Password;
                passwordBox.Visibility = Visibility.Collapsed;
                textBox.Visibility = Visibility.Visible;
                toggleButton.Content = "👁‍🗨";
            }
            else
            {
                passwordBox.Password = textBox.Text;
                textBox.Visibility = Visibility.Collapsed;
                passwordBox.Visibility = Visibility.Visible;
                toggleButton.Content = "👁";
            }
        }

        private string GetPassword(System.Windows.Controls.PasswordBox passwordBox, System.Windows.Controls.TextBox textBox)
        {
            return passwordBox.Visibility == Visibility.Visible ? passwordBox.Password : textBox.Text;
        }

        private class BannerItem
        {
            public string Title { get; set; } = "";
            public string Subtitle { get; set; } = "";
            public string Color { get; set; } = "";
            public Game? Game { get; set; }
        }

        private class GameCardItem
        {
            public string Name { get; set; } = "";
            public string ImagePath { get; set; } = "";
            public string TurkishStatusLabel { get; set; } = "";
            public System.Windows.Media.Brush TurkishStatusColor { get; set; } = System.Windows.Media.Brushes.Gray;
            public Game Game { get; set; } = null!;
        }
    }
}
