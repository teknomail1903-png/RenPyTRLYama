using System;
using System.Collections.Generic;
using System.Linq;
using RenPyTRLauncher.Models;

namespace RenPyTRLauncher.Services
{
    public class InMemorySupportService : ISupportService
    {
        private readonly List<SupportTicket> _tickets = new();

        public IEnumerable<SupportTicket> GetAll() =>
            _tickets.OrderByDescending(t => t.CreatedAt);

        public IEnumerable<SupportTicket> GetForUser(Guid userId) =>
            _tickets.Where(t => t.UserId == userId).OrderByDescending(t => t.CreatedAt);

        public IEnumerable<SupportTicket> GetOpen() =>
            _tickets.Where(t => t.Status == SupportTicketStatus.Open).OrderByDescending(t => t.CreatedAt);

        public SupportTicket? GetById(Guid id) => _tickets.FirstOrDefault(t => t.Id == id);

        public void Create(SupportTicket ticket) => _tickets.Add(ticket);

        public void Reply(Guid ticketId, Guid adminUserId, string reply)
        {
            var t = GetById(ticketId);
            if (t == null) return;
            t.AdminReply = reply;
            t.RepliedByUserId = adminUserId;
            t.RepliedAt = DateTime.UtcNow;
        }

        public void Close(Guid ticketId)
        {
            var t = GetById(ticketId);
            if (t != null) t.Status = SupportTicketStatus.Closed;
        }

        public void Reopen(Guid ticketId)
        {
            var t = GetById(ticketId);
            if (t != null) t.Status = SupportTicketStatus.Open;
        }
    }
}
