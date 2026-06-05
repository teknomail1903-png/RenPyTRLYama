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
            if (game == null)
            {
                _game = new Game();
                _isNew = true;
            }
            else
            {
                _game = game;
                _isNew = false;
            }

            TxtName.Text = _game.Name;
            TxtDesc.Text = _game.Description;
            TxtVersion.Text = _game.Version;
            TxtImage.Text = _game.ImagePath;
            var patchVerBox = this.FindName("TxtPatchVersion") as System.Windows.Controls.TextBox;
            if (patchVerBox != null) patchVerBox.Text = _game.PatchVersion;
            var catBox = this.FindName("TxtCategories") as System.Windows.Controls.TextBox;
            if (catBox != null) catBox.Text = string.Join("; ", _game.Categories);
            var patchBox = this.FindName("TxtPatch") as System.Windows.Controls.TextBox;
            if (patchBox != null) patchBox.Text = _game.PatchFilePath;
            var chkVip = this.FindName("ChkIsVip") as System.Windows.Controls.CheckBox;
            if (chkVip != null) chkVip.IsChecked = _game.IsVip;
            var chkTop10 = this.FindName("ChkIsTop10") as System.Windows.Controls.CheckBox;
            if (chkTop10 != null) chkTop10.IsChecked = _game.IsTop10;
            var chkFeat = this.FindName("ChkIsFeatured") as System.Windows.Controls.CheckBox;
            if (chkFeat != null) chkFeat.IsChecked = _game.IsFeatured;

            BtnCancel.Click += (s, e) => this.Close();
            BtnSave.Click += BtnSave_Click;

            var btnImg = this.FindName("BtnBrowseImage") as System.Windows.Controls.Button;
            if (btnImg != null) btnImg.Click += BtnBrowseImage_Click;
            var btnPatch = this.FindName("BtnBrowsePatch") as System.Windows.Controls.Button;
            if (btnPatch != null) btnPatch.Click += BtnBrowsePatch_Click;
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            _game.Name = TxtName.Text ?? _game.Name;
            _game.Description = TxtDesc.Text ?? _game.Description;
            _game.Version = TxtVersion.Text ?? _game.Version;
            _game.ImagePath = TxtImage.Text ?? _game.ImagePath;
            var patchVerBox = this.FindName("TxtPatchVersion") as System.Windows.Controls.TextBox;
            if (patchVerBox != null) _game.PatchVersion = patchVerBox.Text ?? _game.PatchVersion;
            var catBox = this.FindName("TxtCategories") as System.Windows.Controls.TextBox;
            if (catBox != null)
            {
                _game.Categories = (catBox.Text ?? string.Empty)
                    .Split(new[] { ';', ',' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(c => c.Trim())
                    .Where(c => !string.IsNullOrWhiteSpace(c))
                    .ToList();
            }
            var patchBox = this.FindName("TxtPatch") as System.Windows.Controls.TextBox;
            if (patchBox != null) _game.PatchFilePath = patchBox.Text ?? _game.PatchFilePath;
            _game.IsVip = (this.FindName("ChkIsVip") as System.Windows.Controls.CheckBox)?.IsChecked == true;
            _game.IsTop10 = (this.FindName("ChkIsTop10") as System.Windows.Controls.CheckBox)?.IsChecked == true;
            _game.IsFeatured = (this.FindName("ChkIsFeatured") as System.Windows.Controls.CheckBox)?.IsChecked == true;

            if (_isNew)
            {
                _game.CreatedDate = DateTime.UtcNow;
                _gameService.Add(_game);
            }
            else
            {
                _gameService.Update(_game);
            }
            Services.ServiceLocator.NotifyDataChanged();
            this.Close();
        }

        private void BtnBrowseImage_Click(object? sender, RoutedEventArgs e)
        {
            var dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.Filter = "Image files|*.png;*.jpg;*.jpeg;*.bmp|All files|*.*";
            if (dlg.ShowDialog() == true)
            {
                TxtImage.Text = dlg.FileName;
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
