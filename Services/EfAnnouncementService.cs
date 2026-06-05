using System;
using System.Collections.Generic;
using System.Linq;
using RenPyTRLauncher.Data;
using RenPyTRLauncher.Models;

namespace RenPyTRLauncher.Services
{
    public class EfAnnouncementService : IAnnouncementService
    {
        private readonly AppDbContext _db;
        public EfAnnouncementService(AppDbContext db) { _db = db; }
        public IEnumerable<Announcement> GetAll() => _db.Announcements.OrderByDescending(a => a.CreatedAt).ToList();
        public void Add(Announcement a) { _db.Announcements.Add(a); _db.SaveChanges(); }
        public void Remove(Guid id) { var ex = _db.Announcements.Find(id); if (ex != null) { _db.Announcements.Remove(ex); _db.SaveChanges(); } }
    }
}
