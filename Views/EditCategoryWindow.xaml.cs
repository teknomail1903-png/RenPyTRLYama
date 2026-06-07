using System;
using System.Windows;
using RenPyTRLauncher.Models;
using RenPyTRLauncher.Services;

namespace RenPyTRLauncher.Views
{
    public partial class EditCategoryWindow : Window
    {
        private readonly ICategoryService _categoryService;
        private readonly GameCategory _category;
        private readonly bool _isNew;

        public EditCategoryWindow(GameCategory? category, ICategoryService categoryService)
        {
            InitializeComponent();
            _categoryService = categoryService;
            if (category == null)
            {
                _category = new GameCategory();
                _isNew = true;
            }
            else
            {
                _category = category;
                _isNew = false;
            }

            TxtName.Text = _category.Name;
            TxtDisplayName.Text = _category.DisplayName;
            TxtIcon.Text = _category.Icon;
            TxtColor.Text = _category.AccentColor;
            TxtSortOrder.Text = _category.SortOrder.ToString();
            ChkActive.IsChecked = _category.IsActive;

            BtnCancel.Click += (_, _) => Close();
            BtnSave.Click += BtnSave_Click;
        }

        private void BtnSave_Click(object? sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TxtName.Text) || string.IsNullOrWhiteSpace(TxtDisplayName.Text))
            {
                MessageBox.Show("Kategori anahtarı ve görünen ad zorunludur.", "Kategori", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            _category.Name = TxtName.Text.Trim();
            _category.DisplayName = TxtDisplayName.Text.Trim();
            _category.Icon = string.IsNullOrWhiteSpace(TxtIcon.Text) ? "📁" : TxtIcon.Text.Trim();
            _category.AccentColor = string.IsNullOrWhiteSpace(TxtColor.Text) ? "#3498DB" : TxtColor.Text.Trim();
            _category.IsActive = ChkActive.IsChecked == true;

            if (!int.TryParse(TxtSortOrder.Text, out var sort)) sort = 0;
            _category.SortOrder = sort;

            if (_isNew)
                _categoryService.Add(_category);
            else
                _categoryService.Update(_category);

            ServiceLocator.NotifyDataChanged();
            Close();
        }
    }
}
