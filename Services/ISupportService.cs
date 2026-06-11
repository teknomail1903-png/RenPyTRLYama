using System;
using System.Collections.Generic;
using RenPyTRLauncher.Models;

namespace RenPyTRLauncher.Services
{
    public interface ISupportService
    {
        IEnumerable<SupportTicket> GetAll();
        IEnumerable<SupportTicket> GetForUser(Guid userId);
        IEnumerable<SupportTicket> GetOpen();
        IEnumerable<SupportTicket> GetByStatus(SupportTicketStatus status);
        IEnumerable<SupportTicket> GetByUser(Guid userId);
        SupportTicket? GetById(Guid id);
        void Create(SupportTicket ticket);
        void Update(SupportTicket ticket);
        void Delete(Guid ticketId);
        void AddMessage(Guid ticketId, Guid userId, string message, bool isAdmin);
        void UpdateStatus(Guid ticketId, SupportTicketStatus status);
        void MarkAsReadByAdmin(Guid ticketId);
        void MarkAsReadByUser(Guid ticketId);
        void Close(Guid ticketId);
        void Reopen(Guid ticketId);
    }
}
