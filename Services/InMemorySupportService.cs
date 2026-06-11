using System;
using System.Collections.Generic;
using System.Linq;
using RenPyTRLauncher.Models;

namespace RenPyTRLauncher.Services
{
    public class InMemorySupportService : ISupportService
    {
        private readonly List<SupportTicket> _tickets = new();
        private readonly List<SupportMessage> _messages = new();

        public IEnumerable<SupportTicket> GetAll() =>
            _tickets.OrderByDescending(t => t.CreatedAt);

        public IEnumerable<SupportTicket> GetForUser(Guid userId) =>
            _tickets.Where(t => t.UserId == userId).OrderByDescending(t => t.CreatedAt);

        public IEnumerable<SupportTicket> GetOpen() =>
            _tickets.Where(t => t.Status == SupportTicketStatus.Open).OrderByDescending(t => t.CreatedAt);

        public IEnumerable<SupportTicket> GetByStatus(SupportTicketStatus status) =>
            _tickets.Where(t => t.Status == status).OrderByDescending(t => t.CreatedAt);

        public IEnumerable<SupportTicket> GetByUser(Guid userId) =>
            _tickets.Where(t => t.UserId == userId).OrderByDescending(t => t.CreatedAt);

        public SupportTicket? GetById(Guid id)
        {
            var ticket = _tickets.FirstOrDefault(t => t.Id == id);
            if (ticket != null)
            {
                ticket.Messages = _messages.Where(m => m.SupportTicketId == id).OrderBy(m => m.CreatedAt).ToList();
            }
            return ticket;
        }

        public void Create(SupportTicket ticket) => _tickets.Add(ticket);

        public void Update(SupportTicket ticket)
        {
            var existing = GetById(ticket.Id);
            if (existing == null) return;
            
            existing.Subject = ticket.Subject;
            existing.Type = ticket.Type;
            existing.Status = ticket.Status;
            existing.UpdatedAt = DateTime.UtcNow;
        }

        public void Delete(Guid ticketId)
        {
            var ticket = GetById(ticketId);
            if (ticket == null) return;
            
            _messages.RemoveAll(m => m.SupportTicketId == ticketId);
            _tickets.Remove(ticket);
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

            _messages.Add(supportMessage);
            
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
        }

        public void UpdateStatus(Guid ticketId, SupportTicketStatus status)
        {
            var ticket = GetById(ticketId);
            if (ticket == null) return;
            
            ticket.Status = status;
            ticket.UpdatedAt = DateTime.UtcNow;
        }

        public void MarkAsReadByAdmin(Guid ticketId)
        {
            var ticket = GetById(ticketId);
            if (ticket == null) return;
            
            ticket.IsReadByAdmin = true;
        }

        public void MarkAsReadByUser(Guid ticketId)
        {
            var ticket = GetById(ticketId);
            if (ticket == null) return;
            
            ticket.IsReadByUser = true;
        }

        public void Close(Guid ticketId)
        {
            var ticket = GetById(ticketId);
            if (ticket == null) return;
            
            ticket.Status = SupportTicketStatus.Closed;
            ticket.UpdatedAt = DateTime.UtcNow;
        }

        public void Reopen(Guid ticketId)
        {
            var ticket = GetById(ticketId);
            if (ticket == null) return;
            
            ticket.Status = SupportTicketStatus.Open;
            ticket.UpdatedAt = DateTime.UtcNow;
        }
    }
}
