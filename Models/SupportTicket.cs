using System;

namespace RenPyTRLauncher.Models
{
    public enum SupportTicketType
    {
        General = 0,
        BugReport = 1,
        VipApplication = 2
    }

    public enum SupportTicketStatus
    {
        Open = 0,
        Closed = 1
    }

    public class SupportTicket
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid UserId { get; set; }
        public string Subject { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public SupportTicketType Type { get; set; } = SupportTicketType.General;
        public SupportTicketStatus Status { get; set; } = SupportTicketStatus.Open;
        public string AdminReply { get; set; } = string.Empty;
        public Guid? RepliedByUserId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? RepliedAt { get; set; }

        public string TypeLabel => Type switch
        {
            SupportTicketType.BugReport => "Hata Bildirimi",
            SupportTicketType.VipApplication => "VIP Başvurusu",
            _ => "Destek Talebi"
        };

        public string StatusLabel => Status == SupportTicketStatus.Open ? "Açık" : "Kapalı";
    }
}
