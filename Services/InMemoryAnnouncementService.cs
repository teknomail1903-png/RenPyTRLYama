using System;
using System.Collections.Generic;
using System.Linq;
using RenPyTRLauncher.Models;

namespace RenPyTRLauncher.Services
{
    public class InMemoryAnnouncementService : IAnnouncementService
    {
        private readonly List<Announcement> _anns = new();
        public InMemoryAnnouncementService()
        {
            _anns.Add(new Announcement { Title = "Yeni launcher sürümü", Message = "v1.0 yayınlandı" });
            _anns.Add(new Announcement { Title = "Yeni yama", Message = "Summer Memories için yeni yama eklendi" });
        }
        public IEnumerable<Announcement> GetAll() => _anns.OrderByDescending(a => a.CreatedAt);
        public void Add(Announcement a) => _anns.Add(a);
        public void Update(Announcement a) { }
        public void Remove(Guid id) => _anns.RemoveAll(x => x.Id == id);
    }
}
