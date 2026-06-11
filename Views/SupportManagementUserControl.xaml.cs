using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using RenPyTRLauncher.Models;
using RenPyTRLauncher.Services;

namespace RenPyTRLauncher.Views
{
    public partial class SupportManagementUserControl : UserControl
    {
        private readonly ISupportService _supportService;
        private readonly IUserService _userService;
        private readonly Guid _currentUserId;
        private SupportTicket? _selectedTicket;
        private List<SupportTicket> _allTickets = new();

        public SupportManagementUserControl()
        {
            InitializeComponent();
            _supportService = ServiceLocator.SupportService ?? new InMemorySupportService();
            _userService = ServiceLocator.UserService ?? new InMemoryUserService();
            _currentUserId = SessionService.Current?.UserId ?? Guid.Empty;
            
            InitializeFilters();
            LoadTickets();
            ShowEmptyState();
        }

        private void InitializeFilters()
        {
            CmbStatusFilter.Items.Clear();
            CmbStatusFilter.Items.Add("Tümü");
            CmbStatusFilter.Items.Add("Açık");
            CmbStatusFilter.Items.Add("Yanıtlandı");
            CmbStatusFilter.Items.Add("Kapatıldı");
            CmbStatusFilter.SelectedIndex = 0;

            CmbTicketStatus.Items.Clear();
            CmbTicketStatus.Items.Add("Open");
            CmbTicketStatus.Items.Add("Answered");
            CmbTicketStatus.Items.Add("Closed");
        }

        private void LoadTickets()
        {
            _allTickets = _supportService.GetAll().ToList();
            ApplyFilters();
        }

        private void ApplyFilters()
        {
            var filtered = _allTickets.AsEnumerable();

            // Status filter
            var statusFilter = CmbStatusFilter.SelectedItem?.ToString();
            if (statusFilter != "Tümü")
            {
                var statusEnum = statusFilter switch
                {
                    "Açık" => SupportTicketStatus.Open,
                    "Yanıtlandı" => SupportTicketStatus.Answered,
                    "Kapatıldı" => SupportTicketStatus.Closed,
                    _ => SupportTicketStatus.Open
                };
                filtered = filtered.Where(t => t.Status == statusEnum);
            }

            // User filter
            var userFilter = TxtUserFilter.Text?.ToLower();
            if (!string.IsNullOrWhiteSpace(userFilter))
            {
                filtered = filtered.Where(t => t.User?.Username?.ToLower().Contains(userFilter) == true);
            }

            // Load user info for each ticket
            foreach (var ticket in filtered)
            {
                if (ticket.UserId != Guid.Empty)
                {
                    ticket.User = _userService.GetById(ticket.UserId);
                }
            }

            LstTickets.ItemsSource = filtered.ToList();
        }

        private void ShowEmptyState()
        {
            TicketHeaderPanel.Visibility = Visibility.Collapsed;
            ReplyPanel.Visibility = Visibility.Collapsed;
            EmptyPanel.Visibility = Visibility.Visible;
        }

        private void ShowTicketDetails(SupportTicket ticket)
        {
            _selectedTicket = ticket;
            
            TxtSubject.Text = ticket.Subject;
            TxtStatus.Text = ticket.StatusLabel;
            TxtType.Text = ticket.TypeLabel;
            TxtUser.Text = ticket.User?.Username ?? "Bilinmeyen Kullanıcı";
            
            var color = System.Windows.Media.ColorConverter.ConvertFromString(ticket.StatusColor);
            if (color != null)
            {
                StatusBorder.Background = new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)color);
            }
            
            CmbTicketStatus.SelectedItem = ticket.Status.ToString();
            
            TicketHeaderPanel.Visibility = Visibility.Visible;
            ReplyPanel.Visibility = Visibility.Visible;
            EmptyPanel.Visibility = Visibility.Collapsed;
            
            LoadMessages(ticket);
        }

        private void LoadMessages(SupportTicket ticket)
        {
            MessagesPanel.Children.Clear();
            
            foreach (var message in ticket.Messages.OrderBy(m => m.CreatedAt))
            {
                var bgColor = message.IsAdmin 
                    ? new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromArgb(34, 155, 89, 255))
                    : new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(37, 37, 45));
                
                var fgColor = message.IsAdmin
                    ? new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(155, 89, 255))
                    : new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(160, 165, 184));
                
                var messageBorder = new Border
                {
                    Background = bgColor,
                    CornerRadius = new CornerRadius(8),
                    Padding = new Thickness(12),
                    Margin = new Thickness(0, 0, 0, 8)
                };
                
                var messagePanel = new StackPanel();
                
                var senderText = new TextBlock
                {
                    Text = message.IsAdmin ? "👨‍💼 Destek Ekibi" : $"👤 {ticket.User?.Username ?? "Kullanıcı"}",
                    FontSize = 12,
                    FontWeight = FontWeights.SemiBold,
                    Foreground = fgColor,
                    Margin = new Thickness(0, 0, 0, 4)
                };
                
                var contentText = new TextBlock
                {
                    Text = message.Message,
                    FontSize = 13,
                    Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(224, 224, 232)),
                    TextWrapping = TextWrapping.Wrap
                };
                
                var timeText = new TextBlock
                {
                    Text = message.CreatedAt.ToString("dd.MM.yyyy HH:mm"),
                    FontSize = 11,
                    Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(107, 114, 128)),
                    Margin = new Thickness(0, 4, 0, 0)
                };
                
                messagePanel.Children.Add(senderText);
                messagePanel.Children.Add(contentText);
                messagePanel.Children.Add(timeText);
                
                messageBorder.Child = messagePanel;
                MessagesPanel.Children.Add(messageBorder);
            }
            
            MessagesScrollViewer.ScrollToBottom();
        }

        private void CmbStatusFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplyFilters();
        }

        private void TxtUserFilter_TextChanged(object sender, TextChangedEventArgs e)
        {
            ApplyFilters();
        }

        private void LstTickets_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (LstTickets.SelectedItem is SupportTicket ticket)
            {
                ShowTicketDetails(ticket);
            }
        }

        private void CmbTicketStatus_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_selectedTicket != null && CmbTicketStatus.SelectedItem != null)
            {
                var statusStr = CmbTicketStatus.SelectedItem.ToString();
                var newStatus = statusStr switch
                {
                    "Open" => SupportTicketStatus.Open,
                    "Answered" => SupportTicketStatus.Answered,
                    "Closed" => SupportTicketStatus.Closed,
                    _ => SupportTicketStatus.Open
                };
                
                _supportService.UpdateStatus(_selectedTicket.Id, newStatus);
                
                var updatedTicket = _supportService.GetById(_selectedTicket.Id);
                if (updatedTicket != null)
                {
                    _selectedTicket = updatedTicket;
                    TxtStatus.Text = _selectedTicket.StatusLabel;
                    var color = System.Windows.Media.ColorConverter.ConvertFromString(_selectedTicket.StatusColor);
                    if (color != null)
                    {
                        StatusBorder.Background = new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)color);
                    }
                }
                
                LoadTickets();
            }
        }

        private void BtnSendReply_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedTicket == null || string.IsNullOrWhiteSpace(TxtReply.Text))
                return;
            
            _supportService.AddMessage(_selectedTicket.Id, _currentUserId, TxtReply.Text, true);
            
            // Auto-update status to Answered if it was Open
            if (_selectedTicket.Status == SupportTicketStatus.Open)
            {
                _supportService.UpdateStatus(_selectedTicket.Id, SupportTicketStatus.Answered);
                CmbTicketStatus.SelectedItem = "Answered";
                
                var updatedTicket = _supportService.GetById(_selectedTicket.Id);
                if (updatedTicket != null)
                {
                    _selectedTicket = updatedTicket;
                    TxtStatus.Text = _selectedTicket.StatusLabel;
                    var color = System.Windows.Media.ColorConverter.ConvertFromString(_selectedTicket.StatusColor);
                    if (color != null)
                    {
                        StatusBorder.Background = new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)color);
                    }
                }
                LoadTickets();
            }
            
            TxtReply.Clear();
            
            var reloadedTicket = _supportService.GetById(_selectedTicket.Id);
            if (reloadedTicket != null)
            {
                _selectedTicket = reloadedTicket;
                LoadMessages(_selectedTicket);
            }
        }

        private void BtnDeleteTicket_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedTicket == null)
                return;
            
            var result = MessageBox.Show(
                "Bu destek talebini silmek istediğinize emin misiniz?",
                "Silme Onayı",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);
            
            if (result == MessageBoxResult.Yes)
            {
                _supportService.Delete(_selectedTicket.Id);
                LoadTickets();
                ShowEmptyState();
                _selectedTicket = null;
            }
        }
    }
}
