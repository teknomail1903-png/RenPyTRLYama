using System;
using System.Collections.Generic;
using System.Linq;
using RenPyTRLauncher.Data;
using RenPyTRLauncher.Models;

namespace RenPyTRLauncher.Services
{
    public class EfSupportService : ISupportService
    {
        private readonly AppDbContext _db;

        public EfSupportService(AppDbContext db)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
        }

        public IEnumerable<SupportTicket> GetAll() =>
            _db.SupportTickets.OrderByDescending(t => t.CreatedAt).ToList();

        public IEnumerable<SupportTicket> GetForUser(Guid userId) =>
            _db.SupportTickets.Where(t => t.UserId == userId).OrderByDescending(t => t.CreatedAt).ToList();

        public IEnumerable<SupportTicket> GetOpen() =>
            _db.SupportTickets.Where(t => t.Status == SupportTicketStatus.Open)
                .OrderByDescending(t => t.CreatedAt).ToList();

        public SupportTicket? GetById(Guid id) => _db.SupportTickets.Find(id);

        public void Create(SupportTicket ticket)
        {
            _db.SupportTickets.Add(ticket);
            _db.SaveChanges();
        }

        public void Reply(Guid ticketId, Guid adminUserId, string reply)
        {
            var ticket = GetById(ticketId);
            if (ticket == null) return;
            ticket.AdminReply = reply;
            ticket.RepliedByUserId = adminUserId;
            ticket.RepliedAt = DateTime.UtcNow;
            _db.SaveChanges();
        }

        public void Close(Guid ticketId)
        {
            var ticket = GetById(ticketId);
            if (ticket == null) return;
            ticket.Status = SupportTicketStatus.Closed;
            _db.SaveChanges();
        }

        public void Reopen(Guid ticketId)
        {
            var ticket = GetById(ticketId);
            if (ticket == null) return;
            ticket.Status = SupportTicketStatus.Open;
            _db.SaveChanges();
        }
    }
}
