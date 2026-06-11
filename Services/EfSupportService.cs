using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
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
            _db.SupportTickets
                .Include(t => t.User)
                .Include(t => t.Messages)
                .OrderByDescending(t => t.CreatedAt).ToList();

        public IEnumerable<SupportTicket> GetForUser(Guid userId) =>
            _db.SupportTickets
                .Include(t => t.Messages)
                .Where(t => t.UserId == userId)
                .OrderByDescending(t => t.CreatedAt).ToList();

        public IEnumerable<SupportTicket> GetOpen() =>
            _db.SupportTickets
                .Include(t => t.User)
                .Include(t => t.Messages)
                .Where(t => t.Status == SupportTicketStatus.Open)
                .OrderByDescending(t => t.CreatedAt).ToList();

        public IEnumerable<SupportTicket> GetByStatus(SupportTicketStatus status) =>
            _db.SupportTickets
                .Include(t => t.User)
                .Include(t => t.Messages)
                .Where(t => t.Status == status)
                .OrderByDescending(t => t.CreatedAt).ToList();

        public IEnumerable<SupportTicket> GetByUser(Guid userId) =>
            _db.SupportTickets
                .Include(t => t.Messages)
                .Where(t => t.UserId == userId)
                .OrderByDescending(t => t.CreatedAt).ToList();

        public SupportTicket? GetById(Guid id) =>
            _db.SupportTickets
                .Include(t => t.User)
                .Include(t => t.Messages.OrderBy(m => m.CreatedAt))
                .FirstOrDefault(t => t.Id == id);

        public void Create(SupportTicket ticket)
        {
            _db.SupportTickets.Add(ticket);
            _db.SaveChanges();
        }

        public void Update(SupportTicket ticket)
        {
            var existing = GetById(ticket.Id);
            if (existing == null) return;
            
            existing.Subject = ticket.Subject;
            existing.Type = ticket.Type;
            existing.Status = ticket.Status;
            existing.UpdatedAt = DateTime.UtcNow;
            
            _db.SaveChanges();
        }

        public void Delete(Guid ticketId)
        {
            var ticket = GetById(ticketId);
            if (ticket == null) return;
            
            _db.SupportMessages.RemoveRange(ticket.Messages);
            _db.SupportTickets.Remove(ticket);
            _db.SaveChanges();
        }

        public void AddMessage(Guid ticketId, Guid userId, string message, bool isAdmin)
        {
            var ticket = GetById(ticketId);
            if (ticket == null) return;

            var supportMessage = new SupportMessage
            {
                SupportTicketId = ticketId,
                UserId = userId,
                Message = message,
                IsAdmin = isAdmin,
                CreatedAt = DateTime.UtcNow
            };

            _db.SupportMessages.Add(supportMessage);
            
            ticket.UpdatedAt = DateTime.UtcNow;
            if (isAdmin)
            {
                ticket.Status = SupportTicketStatus.Answered;
                ticket.IsReadByUser = false;
            }
            else
            {
                ticket.IsReadByAdmin = false;
            }

            _db.SaveChanges();
        }

        public void UpdateStatus(Guid ticketId, SupportTicketStatus status)
        {
            var ticket = GetById(ticketId);
            if (ticket == null) return;
            
            ticket.Status = status;
            ticket.UpdatedAt = DateTime.UtcNow;
            _db.SaveChanges();
        }

        public void MarkAsReadByAdmin(Guid ticketId)
        {
            var ticket = GetById(ticketId);
            if (ticket == null) return;
            
            ticket.IsReadByAdmin = true;
            _db.SaveChanges();
        }

        public void MarkAsReadByUser(Guid ticketId)
        {
            var ticket = GetById(ticketId);
            if (ticket == null) return;
            
            ticket.IsReadByUser = true;
            _db.SaveChanges();
        }

        public void Close(Guid ticketId)
        {
            var ticket = GetById(ticketId);
            if (ticket == null) return;
            
            ticket.Status = SupportTicketStatus.Closed;
            ticket.UpdatedAt = DateTime.UtcNow;
            _db.SaveChanges();
        }

        public void Reopen(Guid ticketId)
        {
            var ticket = GetById(ticketId);
            if (ticket == null) return;
            
            ticket.Status = SupportTicketStatus.Open;
            ticket.UpdatedAt = DateTime.UtcNow;
            _db.SaveChanges();
        }
    }
}
