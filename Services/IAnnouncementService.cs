using System.Collections.Generic;
using RenPyTRLauncher.Models;
using System;

namespace RenPyTRLauncher.Services
{
    public interface IAnnouncementService
    {
        IEnumerable<Announcement> GetAll();
        void Add(Announcement a);
        void Remove(Guid id);
    }
}
