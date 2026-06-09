using System;
using System.Linq;
using System.Windows;
using RenPyTRLauncher.Models;
using RenPyTRLauncher.Services;

namespace RenPyTRLauncher.Views
{
    public partial class EditGameWindow : Window
    {
        private readonly IGameService _gameService;
        private Game _game;
        private readonly bool _isNew;

        public EditGameWindow(Game? game, IGameService gameService)
        {
            InitializeComponent();
            _gameService = gameService;
            System.Diagnostics.Debug.WriteLine($"EditGameWindow Constructor - Received game parameter: {game != null}, game.GetHashCode(): {game?.GetHashCode()}");
            if (game == null)
            {
                _game = new Game();
                _game.Id = Guid.NewGuid(); // Generate new Guid only for new games
                _isNew = true;
                System.Diagnostics.Debug.WriteLine($"EditGameWindow opened - New game. Generated Id: {_game.Id}, _game.GetHashCode(): {_game.GetHashCode()}");
            }
            else
            {
                _game = game;
                _isNew = false;
                System.Diagnostics.Debug.WriteLine($"EditGameWindow opened - Edit game. Game.Id: {_game.Id}, Game.Name: {_game.Name}, _game.GetHashCode(): {_game.GetHashCode()}, game.GetHashCode(): {game.GetHashCode()}, Reference equality: {_game == game}");
            }

            TxtName.Text = _game.Name;
            TxtDesc.Text = _game.Description;
            TxtVersion.Text = _game.Version;
            TxtImage.Text = _game.ImagePath;
            var patchVerBox = this.FindName("TxtPatchVersion") as System.Windows.Controls.TextBox;
            if (patchVerBox != null) patchVerBox.Text = _game.PatchVersion;
            LstCategories.ItemsSource = ImageService.GetAvailableCategories();
            foreach (var item in LstCategories.Items)
            {
                if (item is string cat && _game.Categories.Contains(cat))
                    LstCategories.SelectedItems.Add(item);
            }
            var patchBox = this.FindName("TxtPatch") as System.Windows.Controls.TextBox;
            if (patchBox != null) patchBox.Text = _game.PatchFilePath;
            var chkVip = this.FindName("ChkIsVip") as System.Windows.Controls.CheckBox;
            if (chkVip != null) chkVip.IsChecked = _game.IsVip;
            var chkTop10 = this.FindName("ChkIsTop10") as System.Windows.Controls.CheckBox;
            if (chkTop10 != null) chkTop10.IsChecked = _game.IsTop10;
            var chkFeat = this.FindName("ChkIsFeatured") as System.Windows.Controls.CheckBox;
            if (chkFeat != null) chkFeat.IsChecked = _game.IsFeatured;
            TxtPatchNotes.Text = _game.PatchNotes;
            TxtScreenshots.Text = string.Join(Environment.NewLine, _game.ScreenshotPaths);
            TxtDownloadLinks.Text = string.Join(Environment.NewLine, _game.DownloadLinks);

            // Setup game selection ComboBox for patches
            SetupGameSelection();

            BtnCancel.Click += (s, e) => this.Close();
            BtnSave.Click += BtnSave_Click;

            BtnBrowseImage.Click += BtnBrowseImage_Click;
            BtnUrlImage.Click += BtnUrlImage_Click;
            var btnPatch = this.FindName("BtnBrowsePatch") as System.Windows.Controls.Button;
            if (btnPatch != null) btnPatch.Click += BtnBrowsePatch_Click;
            BtnAddScreenshot.Click += BtnAddScreenshot_Click;
        }

        private void SetupGameSelection()
        {
            var lblGameSelection = this.FindName("LblGameSelection") as System.Windows.Controls.TextBlock;
            var cmbGameSelection = this.FindName("CmbGameSelection") as System.Windows.Controls.ComboBox;
            var lblNoGameWarning = this.FindName("LblNoGameWarning") as System.Windows.Controls.TextBlock;

            if (lblGameSelection == null || cmbGameSelection == null || lblNoGameWarning == null) return;

            // Show game selection only for patches
            if (_game.Type == GameType.Patch)
            {
                lblGameSelection.Visibility = Visibility.Visible;
                cmbGameSelection.Visibility = Visibility.Visible;

                // Load games from database (only games, not patches)
                var games = _gameService.GetAll().Where(g => g.Type == GameType.Game).ToList();
                System.Diagnostics.Debug.WriteLine($"SetupGameSelection - Loaded {games.Count} games from database");

                if (games.Count == 0)
                {
                    lblNoGameWarning.Visibility = Visibility.Visible;
                    cmbGameSelection.IsEnabled = false;
                    BtnSave.IsEnabled = false;
                    System.Diagnostics.Debug.WriteLine($"SetupGameSelection - No games found, Save button disabled");
                }
                else
                {
                    lblNoGameWarning.Visibility = Visibility.Collapsed;
                    cmbGameSelection.ItemsSource = games;
                    cmbGameSelection.DisplayMemberPath = "Name";
                    cmbGameSelection.SelectedValuePath = "Id";
                    
                    // If editing an existing patch, select its parent game
                    if (!_isNew && _game.ParentGameId != Guid.Empty)
                    {
                        var parentGame = games.FirstOrDefault(g => g.Id == _game.ParentGameId);
                        if (parentGame != null)
                        {
                            cmbGameSelection.SelectedItem = parentGame;
                        }
                    }

                    cmbGameSelection.SelectionChanged += (s, e) =>
                    {
                        BtnSave.IsEnabled = cmbGameSelection.SelectedItem != null;
                    };

                    // Initially disable Save until a game is selected
                    if (cmbGameSelection.SelectedItem == null)
                    {
                        BtnSave.IsEnabled = false;
                    }
                }
            }
            else
            {
                lblGameSelection.Visibility = Visibility.Collapsed;
                cmbGameSelection.Visibility = Visibility.Collapsed;
                lblNoGameWarning.Visibility = Visibility.Collapsed;
            }
        }

        private void BtnAddScreenshot_Click(object? sender, RoutedEventArgs e)
        {
            var dlg = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "Resim|*.png;*.jpg;*.jpeg;*.webp;*.bmp"
            };
            if (dlg.ShowDialog() != true) return;
            try
            {
                var path = ImageService.UploadFromFile(dlg.FileName, ImageCategory.Games);
                TxtScreenshots.Text = string.IsNullOrWhiteSpace(TxtScreenshots.Text)
                    ? path
                    : TxtScreenshots.Text + Environment.NewLine + path;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Yükleme Hatası", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine($"BtnSave_Click - Before save. Game.Id: {_game.Id}, Game.Name: {_game.Name}, _isNew: {_isNew}, _game.GetHashCode(): {_game.GetHashCode()}");

            // For patches, validate game selection
            if (_game.Type == GameType.Patch)
            {
                var cmbGameSelection = this.FindName("CmbGameSelection") as System.Windows.Controls.ComboBox;
                if (cmbGameSelection != null && cmbGameSelection.SelectedItem is Game selectedGame)
                {
                    _game.ParentGameId = selectedGame.Id;
                    System.Diagnostics.Debug.WriteLine($"BtnSave_Click - Patch parent game selected: {selectedGame.Id}, {selectedGame.Name}");

                    // Verify the selected game exists in database
                    var gameInDb = _gameService.GetById(selectedGame.Id);
                    if (gameInDb == null)
                    {
                        MessageBox.Show("Seçilen oyun bulunamadı.", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                }
                else if (_isNew)
                {
                    MessageBox.Show("Lütfen bir oyun seçin.", "Hata", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
            }

            _game.Name = TxtName.Text ?? _game.Name;
            _game.Description = TxtDesc.Text ?? _game.Description;
            _game.Version = TxtVersion.Text ?? _game.Version;
            _game.ImagePath = TxtImage.Text ?? _game.ImagePath;
            var patchVerBox = this.FindName("TxtPatchVersion") as System.Windows.Controls.TextBox;
            if (patchVerBox != null) _game.PatchVersion = patchVerBox.Text ?? _game.PatchVersion;
            _game.Categories = LstCategories.SelectedItems.Cast<string>().ToList();
            var patchBox = this.FindName("TxtPatch") as System.Windows.Controls.TextBox;
            if (patchBox != null) _game.PatchFilePath = patchBox.Text ?? _game.PatchFilePath;
            _game.IsVip = (this.FindName("ChkIsVip") as System.Windows.Controls.CheckBox)?.IsChecked == true;
            _game.IsTop10 = (this.FindName("ChkIsTop10") as System.Windows.Controls.CheckBox)?.IsChecked == true;
            _game.IsFeatured = (this.FindName("ChkIsFeatured") as System.Windows.Controls.CheckBox)?.IsChecked == true;
            _game.PatchNotes = TxtPatchNotes.Text ?? "";
            _game.ScreenshotPaths = TxtScreenshots.Text
                .Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(s => s.Trim()).Where(s => !string.IsNullOrEmpty(s)).ToList();
            _game.DownloadLinks = TxtDownloadLinks.Text
                .Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(s => s.Trim()).Where(s => !string.IsNullOrEmpty(s)).ToList();

            System.Diagnostics.Debug.WriteLine($"BtnSave_Click - After field updates. Game.Id: {_game.Id}, Game.Name: {_game.Name}, _game.GetHashCode(): {_game.GetHashCode()}");

            if (_isNew)
            {
                _gameService.Add(_game);
                System.Diagnostics.Debug.WriteLine($"BtnSave_Click - After Add. Game.Id: {_game.Id}, Game.Name: {_game.Name}, _game.GetHashCode(): {_game.GetHashCode()}");
            }
            else
            {
                _gameService.Update(_game);
                System.Diagnostics.Debug.WriteLine($"BtnSave_Click - After Update. Game.Id: {_game.Id}, Game.Name: {_game.Name}, _game.GetHashCode(): {_game.GetHashCode()}");
                ServiceLocator.NotificationService?.NotifyAllUsers(
                    "Yama Güncellendi",
                    $"{_game.Name} için yeni yama: {_game.PatchVersion}",
                    NotificationType.NewPatch,
                    _game.Id);
            }
            Services.ServiceLocator.NotifyDataChanged();
            this.Close();
        }

        private void BtnBrowseImage_Click(object? sender, RoutedEventArgs e)
        {
            var dlg = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "Resim|*.png;*.jpg;*.jpeg;*.webp;*.bmp|Tümü|*.*"
            };
            if (dlg.ShowDialog() != true) return;
            try
            {
                TxtImage.Text = ImageService.UploadFromFile(dlg.FileName, ImageCategory.Games);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Yükleme Hatası", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void BtnUrlImage_Click(object? sender, RoutedEventArgs e)
        {
            var dlg = new InputDialog("Kapak görseli URL'sini girin:", TxtImage.Text) { Owner = this };
            if (dlg.ShowDialog() == true && !string.IsNullOrWhiteSpace(dlg.Result))
                TxtImage.Text = dlg.Result.Trim();
        }

        private void BtnBrowsePatch_Click(object? sender, RoutedEventArgs e)
        {
            var dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.Filter = "Patch files|*.zip;*.7z;*.rar;*.patch|All files|*.*";
            if (dlg.ShowDialog() == true)
            {
                var patchTb = this.FindName("TxtPatch") as System.Windows.Controls.TextBox;
                if (patchTb != null) patchTb.Text = dlg.FileName;
                else System.Windows.MessageBox.Show($"Seçilen yama: {dlg.FileName}", "Yama Seçildi", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
    }
}
