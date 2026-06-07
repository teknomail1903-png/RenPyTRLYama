using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.Win32;
using RenPyTRLauncher.Models;
using RenPyTRLauncher.Services;

namespace RenPyTRLauncher.Views
{
    public partial class EditProfileWindow : Window
    {
        private readonly User _user;
        private string _avatarPath = "";

        public EditProfileWindow(User user)
        {
            InitializeComponent();
            _user = user;
            _avatarPath = user.AvatarPath;
            TxtUsername.Text = user.Username;
            TxtEmail.Text = user.Email;
            TxtCity.Text = user.City;
            TxtAge.Text = user.Age?.ToString() ?? "";

            LoadAvatarPreview();
            BuildAvatarPicker();

            BtnCancel.Click += (_, _) => Close();
            BtnSave.Click += BtnSave_Click;
            BtnUploadAvatar.Click += BtnUploadAvatar_Click;
        }

        private void LoadAvatarPreview()
        {
            var path = ImageService.ResolvePath(_avatarPath);
            if (!string.IsNullOrEmpty(path) && !path.StartsWith("avatar://"))
            {
                try
                {
                    ImgAvatar.Source = new System.Windows.Media.Imaging.BitmapImage(new Uri(path, UriKind.RelativeOrAbsolute));
                }
                catch { ImgAvatar.Source = null; }
            }
            else
            {
                ImgAvatar.Source = null;
            }
        }

        private void BuildAvatarPicker()
        {
            AvatarPicker.Children.Clear();
            for (var i = 1; i <= 8; i++)
            {
                var idx = i;
                var btn = new Button
                {
                    Width = 36,
                    Height = 36,
                    Margin = new Thickness(0, 0, 6, 6),
                    Background = new SolidColorBrush(Color.FromRgb(0x2A, 0x2A, 0x35)),
                    BorderThickness = new Thickness(0),
                    Content = GetAvatarEmoji(idx),
                    FontSize = 18,
                    Cursor = System.Windows.Input.Cursors.Hand,
                    Tag = idx
                };
                btn.Click += (_, _) =>
                {
                    _avatarPath = $"avatar://default_{idx}";
                    LoadAvatarPreview();
                };
                AvatarPicker.Children.Add(btn);
            }
        }

        private static string GetAvatarEmoji(int index) => index switch
        {
            1 => "😀", 2 => "😎", 3 => "🦊", 4 => "🐱",
            5 => "🐻", 6 => "🎮", 7 => "🌟", _ => "💎"
        };

        private void BtnUploadAvatar_Click(object? sender, RoutedEventArgs e)
        {
            var dlg = new OpenFileDialog { Filter = "Resim|*.png;*.jpg;*.jpeg;*.webp;*.bmp" };
            if (dlg.ShowDialog() != true) return;
            try
            {
                _avatarPath = ImageService.UploadFromFile(dlg.FileName, ImageCategory.Avatars);
                LoadAvatarPreview();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Avatar Yükleme", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void BtnSave_Click(object? sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TxtEmail.Text))
            {
                MessageBox.Show("E-posta alanı zorunludur.", "Profil", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            int? age = null;
            if (!string.IsNullOrWhiteSpace(TxtAge.Text))
            {
                if (!int.TryParse(TxtAge.Text, out var parsed) || parsed < 1 || parsed > 120)
                {
                    MessageBox.Show("Geçerli bir yaş girin (1-120).", "Profil", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                age = parsed;
            }

            _user.Email = TxtEmail.Text.Trim();
            _user.City = TxtCity.Text?.Trim() ?? "";
            _user.Age = age;
            _user.AvatarPath = _avatarPath;

            ServiceLocator.UserService?.Update(_user);
            ServiceLocator.NotifyDataChanged();
            Close();
        }
    }
}
