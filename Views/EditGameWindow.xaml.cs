using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
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

            // Load cover image preview
            if (!string.IsNullOrEmpty(_game.ImagePath))
            {
                UpdateCoverPreview(_game.ImagePath);
            }

            // Load screenshot preview
            if (_game.ScreenshotPaths.Count > 0)
            {
                UpdateScreenshotPreview();
            }

            // Load tags
            LoadTags();

            // Load new fields
            LoadNewFields();

            // Setup game selection ComboBox for patches
            SetupGameSelection();

            BtnCancel.Click += (s, e) => this.Close();
            BtnSave.Click += BtnSave_Click;

            BtnBrowseImage.Click += BtnBrowseImage_Click;
            BtnUrlImage.Click += BtnUrlImage_Click;
            var btnPatch = this.FindName("BtnBrowsePatch") as System.Windows.Controls.Button;
            if (btnPatch != null) btnPatch.Click += BtnBrowsePatch_Click;
            BtnAddScreenshot.Click += BtnAddScreenshot_Click;

            // Tag management
            var btnAddTag = this.FindName("BtnAddTag") as System.Windows.Controls.Button;
            if (btnAddTag != null) btnAddTag.Click += BtnAddTag_Click;

            // New field management
            var btnAddGameGenre = this.FindName("BtnAddGameGenre") as System.Windows.Controls.Button;
            if (btnAddGameGenre != null) btnAddGameGenre.Click += BtnAddGameGenre_Click;
            var btnAddPlatform = this.FindName("BtnAddPlatform") as System.Windows.Controls.Button;
            if (btnAddPlatform != null) btnAddPlatform.Click += BtnAddPlatform_Click;
            var btnAddContentWarning = this.FindName("BtnAddContentWarning") as System.Windows.Controls.Button;
            if (btnAddContentWarning != null) btnAddContentWarning.Click += BtnAddContentWarning_Click;
        }

        private void LoadTags()
        {
            if (_game.Tags != null && _game.Tags.Count > 0)
            {
                TagList.ItemsSource = new System.Collections.ObjectModel.ObservableCollection<string>(_game.Tags);
            }
            else
            {
                TagList.ItemsSource = new System.Collections.ObjectModel.ObservableCollection<string>();
            }
        }

        private void LoadNewFields()
        {
            // Load Turkish Status
            var cmbTurkishStatus = this.FindName("CmbTurkishStatus") as System.Windows.Controls.ComboBox;
            if (cmbTurkishStatus != null)
            {
                foreach (System.Windows.Controls.ComboBoxItem item in cmbTurkishStatus.Items)
                {
                    if (item.Tag != null && item.Tag.ToString() == _game.TurkishStatus.ToString())
                    {
                        cmbTurkishStatus.SelectedItem = item;
                        break;
                    }
                }
            }

            // Load Steam Status
            var cmbSteamStatus = this.FindName("CmbSteamStatus") as System.Windows.Controls.ComboBox;
            if (cmbSteamStatus != null)
            {
                foreach (System.Windows.Controls.ComboBoxItem item in cmbSteamStatus.Items)
                {
                    if (item.Tag != null && item.Tag.ToString() == _game.SteamStatus.ToString())
                    {
                        cmbSteamStatus.SelectedItem = item;
                        break;
                    }
                }
            }

            // Load Completion Status
            var cmbCompletionStatus = this.FindName("CmbCompletionStatus") as System.Windows.Controls.ComboBox;
            if (cmbCompletionStatus != null)
            {
                foreach (System.Windows.Controls.ComboBoxItem item in cmbCompletionStatus.Items)
                {
                    if (item.Tag != null && item.Tag.ToString() == _game.CompletionStatus.ToString())
                    {
                        cmbCompletionStatus.SelectedItem = item;
                        break;
                    }
                }
            }

            // Load text fields
            var txtDeveloper = this.FindName("TxtDeveloper") as System.Windows.Controls.TextBox;
            if (txtDeveloper != null) txtDeveloper.Text = _game.Developer;

            var txtPublisher = this.FindName("TxtPublisher") as System.Windows.Controls.TextBox;
            if (txtPublisher != null) txtPublisher.Text = _game.Publisher;

            var txtGameEngine = this.FindName("TxtGameEngine") as System.Windows.Controls.TextBox;
            if (txtGameEngine != null) txtGameEngine.Text = _game.GameEngine;

            var dtpReleaseDate = this.FindName("DtpReleaseDate") as System.Windows.Controls.DatePicker;
            if (dtpReleaseDate != null && _game.ReleaseDate.HasValue)
            {
                dtpReleaseDate.SelectedDate = _game.ReleaseDate.Value;
            }

            var txtAveragePlaytime = this.FindName("TxtAveragePlaytime") as System.Windows.Controls.TextBox;
            if (txtAveragePlaytime != null) txtAveragePlaytime.Text = _game.AveragePlaytime;

            // Load Game Genres
            if (_game.GameGenres != null && _game.GameGenres.Count > 0)
            {
                GameGenresList.ItemsSource = new System.Collections.ObjectModel.ObservableCollection<string>(_game.GameGenres);
            }
            else
            {
                GameGenresList.ItemsSource = new System.Collections.ObjectModel.ObservableCollection<string>();
            }

            // Load Platforms
            if (_game.Platforms != null && _game.Platforms.Count > 0)
            {
                PlatformsList.ItemsSource = new System.Collections.ObjectModel.ObservableCollection<string>(_game.Platforms);
            }
            else
            {
                PlatformsList.ItemsSource = new System.Collections.ObjectModel.ObservableCollection<string>();
            }

            // Load Content Warnings
            if (_game.ContentWarnings != null && _game.ContentWarnings.Count > 0)
            {
                ContentWarningsList.ItemsSource = new System.Collections.ObjectModel.ObservableCollection<string>(_game.ContentWarnings);
            }
            else
            {
                ContentWarningsList.ItemsSource = new System.Collections.ObjectModel.ObservableCollection<string>();
            }

            // Load Game Type
            var cmbGameType = this.FindName("CmbGameType") as System.Windows.Controls.ComboBox;
            if (cmbGameType != null)
            {
                foreach (System.Windows.Controls.ComboBoxItem item in cmbGameType.Items)
                {
                    if (item.Tag != null && item.Tag.ToString() == _game.Type.ToString())
                    {
                        cmbGameType.SelectedItem = item;
                        break;
                    }
                }
                cmbGameType.SelectionChanged += CmbGameType_SelectionChanged;
            }
        }

        private void BtnAddTag_Click(object? sender, RoutedEventArgs e)
        {
            var txtNewTag = this.FindName("TxtNewTag") as System.Windows.Controls.TextBox;
            if (txtNewTag == null || string.IsNullOrWhiteSpace(txtNewTag.Text)) return;

            var newTag = txtNewTag.Text.Trim();
            if (_game.Tags.Contains(newTag))
            {
                MessageBox.Show("Bu etiket zaten mevcut.", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            _game.Tags.Add(newTag);
            TagList.ItemsSource = new System.Collections.ObjectModel.ObservableCollection<string>(_game.Tags);
            txtNewTag.Text = "";
        }

        private void RemoveTag_Click(object sender, RoutedEventArgs e)
        {
            if (sender is System.Windows.Controls.Button btn && btn.Tag is string tagToRemove)
            {
                _game.Tags.Remove(tagToRemove);
                TagList.ItemsSource = new System.Collections.ObjectModel.ObservableCollection<string>(_game.Tags);
            }
        }

        private void RemoveGameGenre_Click(object sender, RoutedEventArgs e)
        {
            if (sender is System.Windows.Controls.Button btn && btn.Tag is string genreToRemove)
            {
                _game.GameGenres.Remove(genreToRemove);
                GameGenresList.ItemsSource = new System.Collections.ObjectModel.ObservableCollection<string>(_game.GameGenres);
            }
        }

        private void RemovePlatform_Click(object sender, RoutedEventArgs e)
        {
            if (sender is System.Windows.Controls.Button btn && btn.Tag is string platformToRemove)
            {
                _game.Platforms.Remove(platformToRemove);
                PlatformsList.ItemsSource = new System.Collections.ObjectModel.ObservableCollection<string>(_game.Platforms);
            }
        }

        private void RemoveContentWarning_Click(object sender, RoutedEventArgs e)
        {
            if (sender is System.Windows.Controls.Button btn && btn.Tag is string warningToRemove)
            {
                _game.ContentWarnings.Remove(warningToRemove);
                ContentWarningsList.ItemsSource = new System.Collections.ObjectModel.ObservableCollection<string>(_game.ContentWarnings);
            }
        }

        private void BtnAddGameGenre_Click(object sender, RoutedEventArgs e)
        {
            var cmbGameGenre = this.FindName("CmbGameGenre") as System.Windows.Controls.ComboBox;
            if (cmbGameGenre == null || cmbGameGenre.SelectedItem == null) return;

            var newGenre = cmbGameGenre.SelectedItem.ToString();
            if (_game.GameGenres.Contains(newGenre))
            {
                MessageBox.Show("Bu oyun türü zaten mevcut.", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            _game.GameGenres.Add(newGenre);
            GameGenresList.ItemsSource = new System.Collections.ObjectModel.ObservableCollection<string>(_game.GameGenres);
        }

        private void BtnAddPlatform_Click(object sender, RoutedEventArgs e)
        {
            var cmbPlatform = this.FindName("CmbPlatform") as System.Windows.Controls.ComboBox;
            if (cmbPlatform == null || cmbPlatform.SelectedItem == null) return;

            var newPlatform = cmbPlatform.SelectedItem.ToString();
            if (_game.Platforms.Contains(newPlatform))
            {
                MessageBox.Show("Bu platform zaten mevcut.", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            _game.Platforms.Add(newPlatform);
            PlatformsList.ItemsSource = new System.Collections.ObjectModel.ObservableCollection<string>(_game.Platforms);
        }

        private void BtnAddContentWarning_Click(object sender, RoutedEventArgs e)
        {
            var cmbContentWarning = this.FindName("CmbContentWarning") as System.Windows.Controls.ComboBox;
            if (cmbContentWarning == null || cmbContentWarning.SelectedItem == null) return;

            var newWarning = cmbContentWarning.SelectedItem.ToString();
            if (_game.ContentWarnings.Contains(newWarning))
            {
                MessageBox.Show("Bu içerik uyarısı zaten mevcut.", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            _game.ContentWarnings.Add(newWarning);
            ContentWarningsList.ItemsSource = new System.Collections.ObjectModel.ObservableCollection<string>(_game.ContentWarnings);
        }

        private void CmbGameType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var cmbGameType = sender as System.Windows.Controls.ComboBox;
            if (cmbGameType == null || cmbGameType.SelectedItem == null) return;

            var selectedItem = cmbGameType.SelectedItem as System.Windows.Controls.ComboBoxItem;
            if (selectedItem == null || selectedItem.Tag == null) return;

            var selectedTypeStr = selectedItem.Tag.ToString();
            if (Enum.TryParse<GameType>(selectedTypeStr, out var selectedType))
            {
                _game.Type = selectedType;
                SetupGameSelection();
            }
        }

        private void SetupGameSelection()
        {
            var lblGameSelection = this.FindName("LblGameSelection") as System.Windows.Controls.TextBlock;
            var cmbGameSelection = this.FindName("CmbGameSelection") as System.Windows.Controls.ComboBox;
            var lblNoGameWarning = this.FindName("LblNoGameWarning") as System.Windows.Controls.TextBlock;

            if (lblGameSelection == null || cmbGameSelection == null || lblNoGameWarning == null) return;

            // Show game selection only for mod types (not games)
            bool isModType = _game.Type == GameType.Translation ||
                            _game.Type == GameType.Gallery ||
                            _game.Type == GameType.Cheat ||
                            _game.Type == GameType.Walkthrough ||
                            _game.Type == GameType.Save ||
                            _game.Type == GameType.Extra;

            if (isModType)
            {
                lblGameSelection.Visibility = Visibility.Visible;
                cmbGameSelection.Visibility = Visibility.Visible;

                // Load games from database (only games, not mods)
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

                    // If editing an existing mod, select its parent game
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
                BtnSave.IsEnabled = true;
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
                UpdateScreenshotPreview();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Yükleme Hatası", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void UpdateScreenshotPreview()
        {
            var previewBorder = this.FindName("ImgScreenshotsPreview") as System.Windows.Controls.Border;
            if (previewBorder == null) return;

            try
            {
                var screenshotPaths = TxtScreenshots.Text
                    .Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(s => s.Trim())
                    .Where(s => !string.IsNullOrEmpty(s))
                    .ToList();

                if (screenshotPaths.Count > 0)
                {
                    var firstScreenshot = screenshotPaths[0];
                    var resolvedPath = ImageService.ResolvePath(firstScreenshot);
                    if (!string.IsNullOrEmpty(resolvedPath))
                    {
                        previewBorder.Child = new System.Windows.Controls.Image
                        {
                            Source = new System.Windows.Media.Imaging.BitmapImage(new Uri(resolvedPath, UriKind.RelativeOrAbsolute)),
                            Stretch = System.Windows.Media.Stretch.UniformToFill
                        };
                    }
                }
            }
            catch
            {
                // Keep placeholder if preview fails
            }
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine($"BtnSave_Click - Before save. Game.Id: {_game.Id}, Game.Name: {_game.Name}, _isNew: {_isNew}, _game.GetHashCode(): {_game.GetHashCode()}");

            // Save GameType from ComboBox
            var cmbGameType = this.FindName("CmbGameType") as System.Windows.Controls.ComboBox;
            if (cmbGameType != null && cmbGameType.SelectedItem is System.Windows.Controls.ComboBoxItem gameTypeItem && gameTypeItem.Tag != null)
            {
                if (Enum.TryParse<GameType>(gameTypeItem.Tag.ToString(), out var selectedType))
                {
                    _game.Type = selectedType;
                }
            }

            // For mod types, validate game selection
            bool isModType = _game.Type == GameType.Translation ||
                            _game.Type == GameType.Gallery ||
                            _game.Type == GameType.Cheat ||
                            _game.Type == GameType.Walkthrough ||
                            _game.Type == GameType.Save ||
                            _game.Type == GameType.Extra;

            if (isModType)
            {
                var cmbGameSelection = this.FindName("CmbGameSelection") as System.Windows.Controls.ComboBox;
                if (cmbGameSelection != null && cmbGameSelection.SelectedItem is Game selectedGame)
                {
                    _game.ParentGameId = selectedGame.Id;
                    System.Diagnostics.Debug.WriteLine($"BtnSave_Click - Mod parent game selected: {selectedGame.Id}, {selectedGame.Name}");

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

            // Save new fields
            var cmbTurkishStatus = this.FindName("CmbTurkishStatus") as System.Windows.Controls.ComboBox;
            if (cmbTurkishStatus != null && cmbTurkishStatus.SelectedItem is System.Windows.Controls.ComboBoxItem turkishItem)
            {
                _game.TurkishStatus = Enum.Parse<TurkishStatus>(turkishItem.Tag.ToString());
            }

            var cmbSteamStatus = this.FindName("CmbSteamStatus") as System.Windows.Controls.ComboBox;
            if (cmbSteamStatus != null && cmbSteamStatus.SelectedItem is System.Windows.Controls.ComboBoxItem steamItem)
            {
                _game.SteamStatus = Enum.Parse<SteamStatus>(steamItem.Tag.ToString());
            }

            var cmbCompletionStatus = this.FindName("CmbCompletionStatus") as System.Windows.Controls.ComboBox;
            if (cmbCompletionStatus != null && cmbCompletionStatus.SelectedItem is System.Windows.Controls.ComboBoxItem completionItem)
            {
                _game.CompletionStatus = Enum.Parse<CompletionStatus>(completionItem.Tag.ToString());
            }

            var txtDeveloper = this.FindName("TxtDeveloper") as System.Windows.Controls.TextBox;
            if (txtDeveloper != null) _game.Developer = txtDeveloper.Text ?? string.Empty;

            var txtPublisher = this.FindName("TxtPublisher") as System.Windows.Controls.TextBox;
            if (txtPublisher != null) _game.Publisher = txtPublisher.Text ?? string.Empty;

            var txtGameEngine = this.FindName("TxtGameEngine") as System.Windows.Controls.TextBox;
            if (txtGameEngine != null) _game.GameEngine = txtGameEngine.Text ?? string.Empty;

            var dtpReleaseDate = this.FindName("DtpReleaseDate") as System.Windows.Controls.DatePicker;
            if (dtpReleaseDate != null)
            {
                _game.ReleaseDate = dtpReleaseDate.SelectedDate;
            }

            var txtAveragePlaytime = this.FindName("TxtAveragePlaytime") as System.Windows.Controls.TextBox;
            if (txtAveragePlaytime != null) _game.AveragePlaytime = txtAveragePlaytime.Text ?? string.Empty;

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
                UpdateCoverPreview(TxtImage.Text);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Yükleme Hatası", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void UpdateCoverPreview(string imagePath)
        {
            var previewBorder = this.FindName("ImgCoverPreview") as System.Windows.Controls.Border;
            if (previewBorder == null) return;

            try
            {
                var resolvedPath = ImageService.ResolvePath(imagePath);
                if (!string.IsNullOrEmpty(resolvedPath))
                {
                    previewBorder.Child = new System.Windows.Controls.Image
                    {
                        Source = new System.Windows.Media.Imaging.BitmapImage(new Uri(resolvedPath, UriKind.RelativeOrAbsolute)),
                        Stretch = System.Windows.Media.Stretch.UniformToFill
                    };
                }
            }
            catch
            {
                // Keep placeholder if preview fails
            }
        }

        private void BtnUrlImage_Click(object? sender, RoutedEventArgs e)
        {
            var dlg = new InputDialog("Kapak görseli URL'sini girin:", TxtImage.Text) { Owner = this };
            if (dlg.ShowDialog() == true && !string.IsNullOrWhiteSpace(dlg.Result))
            {
                TxtImage.Text = dlg.Result.Trim();
                UpdateCoverPreview(TxtImage.Text);
            }
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
