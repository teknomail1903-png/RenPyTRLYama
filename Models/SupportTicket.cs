using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace RenPyTRLauncher.Models
{
    public enum SupportTicketType
    {
        General = 0,
        BugReport = 1,
        VipApplication = 2,
        AccountIssue = 3,
        TechnicalSupport = 4
    }

    public enum SupportTicketStatus
    {
        Open = 0,
        Pending = 1,
        Answered = 2,
        Resolved = 3,
        Closed = 4
    }

    public class SupportTicket
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid UserId { get; set; }
        public string Subject { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public SupportTicketType Type { get; set; } = SupportTicketType.General;
        public SupportTicketStatus Status { get; set; } = SupportTicketStatus.Open;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public bool IsReadByAdmin { get; set; } = false;
        public bool IsReadByUser { get; set; } = true;

        // Navigation properties
        public User? User { get; set; }
        public ICollection<SupportMessage> Messages { get; set; } = new List<SupportMessage>();

        public string TypeLabel => Type switch
        {
            SupportTicketType.BugReport => "Hata Bildirimi",
            SupportTicketType.VipApplication => "VIP Başvurusu",
            SupportTicketType.AccountIssue => "Hesap Sorunu",
            SupportTicketType.TechnicalSupport => "Teknik Destek",
            _ => "Destek Talebi"
        };

        public string StatusLabel => Status switch
        {
            SupportTicketStatus.Open => "Açık",
            SupportTicketStatus.Pending => "Beklemede",
            SupportTicketStatus.Answered => "Cevaplandı",
            SupportTicketStatus.Resolved => "Çözüldü",
            SupportTicketStatus.Closed => "Kapalı",
            _ => "Bilinmiyor"
        };

        [NotMapped]
        public string StatusColor => Status switch
        {
            SupportTicketStatus.Open => "#EF4444",
            SupportTicketStatus.Pending => "#F59E0B",
            SupportTicketStatus.Answered => "#3B82F6",
            SupportTicketStatus.Resolved => "#10B981",
            SupportTicketStatus.Closed => "#6B7280",
            _ => "#6B7280"
        };
    }

    public class SupportMessage
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid SupportTicketId { get; set; }
        public Guid UserId { get; set; }
        public string Message { get; set; } = string.Empty;
        public bool IsAdmin { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsRead { get; set; } = false;

        // Navigation properties
        public SupportTicket? SupportTicket { get; set; }
        public User? User { get; set; }
    }
}
