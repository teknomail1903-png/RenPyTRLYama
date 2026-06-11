using System;
using System.Windows;
using System.Windows.Controls;
using RenPyTRLauncher.Models;
using RenPyTRLauncher.Services;

namespace RenPyTRLauncher.Views
{
    public partial class SupportUserControl : UserControl
    {
        private readonly ISupportService _supportService;
        private readonly Guid _currentUserId;
        private SupportTicket? _selectedTicket;

        public SupportUserControl()
        {
            InitializeComponent();
            _supportService = ServiceLocator.SupportService ?? new InMemorySupportService();
            _currentUserId = SessionService.Current?.UserId ?? Guid.Empty;
            
            LoadTickets();
            ShowEmptyState();
        }

        private void LoadTickets()
        {
            var tickets = _supportService.GetForUser(_currentUserId);
            LstTickets.ItemsSource = tickets;
        }

        private void ShowEmptyState()
        {
            TicketHeaderPanel.Visibility = Visibility.Collapsed;
            ReplyPanel.Visibility = Visibility.Collapsed;
            NewTicketPanel.Visibility = Visibility.Collapsed;
            EmptyPanel.Visibility = Visibility.Visible;
        }

        private void ShowTicketDetails(SupportTicket ticket)
        {
            _selectedTicket = ticket;
            
            TxtSubject.Text = ticket.Subject;
            TxtStatus.Text = ticket.StatusLabel;
            TxtType.Text = ticket.TypeLabel;
            
            TicketHeaderPanel.Visibility = Visibility.Visible;
            ReplyPanel.Visibility = Visibility.Visible;
            NewTicketPanel.Visibility = Visibility.Collapsed;
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
                    Text = message.IsAdmin ? "👨‍💼 Destek Ekibi" : "👤 Siz",
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

        private void BtnNewTicket_Click(object sender, RoutedEventArgs e)
        {
            TicketHeaderPanel.Visibility = Visibility.Collapsed;
            ReplyPanel.Visibility = Visibility.Collapsed;
            NewTicketPanel.Visibility = Visibility.Visible;
            EmptyPanel.Visibility = Visibility.Collapsed;
            
            CmbTicketType.Items.Clear();
            CmbTicketType.Items.Add("Teknik Sorun");
            CmbTicketType.Items.Add("Öneri");
            CmbTicketType.Items.Add("Şikayet");
            CmbTicketType.Items.Add("Diğer");
            CmbTicketType.SelectedIndex = 0;
            
            TxtNewSubject.Clear();
            TxtNewMessage.Clear();
        }

        private void BtnSubmitTicket_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TxtNewSubject.Text))
            {
                MessageBox.Show("Lütfen bir konu girin.", "Hata", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            
            if (string.IsNullOrWhiteSpace(TxtNewMessage.Text))
            {
                MessageBox.Show("Lütfen bir mesaj girin.", "Hata", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            
            var ticket = new SupportTicket
            {
                Id = Guid.NewGuid(),
                UserId = _currentUserId,
                Subject = TxtNewSubject.Text,
                Message = TxtNewMessage.Text,
                Type = SupportTicketType.TechnicalSupport,
                Status = SupportTicketStatus.Open,
                CreatedAt = DateTime.UtcNow
            };
            
            _supportService.Create(ticket);
            
            LoadTickets();
            ShowEmptyState();
            
            MessageBox.Show("Destek talebiniz oluşturuldu.", "Başarılı", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void LstTickets_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (LstTickets.SelectedItem is SupportTicket ticket)
            {
                ShowTicketDetails(ticket);
            }
        }

        private void BtnSendReply_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedTicket == null || string.IsNullOrWhiteSpace(TxtReply.Text))
                return;
            
            _supportService.AddMessage(_selectedTicket.Id, _currentUserId, TxtReply.Text, false);
            
            TxtReply.Clear();
            
            // Reload ticket to get updated messages
            var updatedTicket = _supportService.GetById(_selectedTicket.Id);
            if (updatedTicket != null)
            {
                _selectedTicket = updatedTicket;
                LoadMessages(_selectedTicket);
            }
        }
    }
}
