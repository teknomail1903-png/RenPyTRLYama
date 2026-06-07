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
        SupportTicket? GetById(Guid id);
        void Create(SupportTicket ticket);
        void Reply(Guid ticketId, Guid adminUserId, string reply);
        void Close(Guid ticketId);
        void Reopen(Guid ticketId);
    }
}
