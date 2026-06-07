using System;
using System.Collections.Generic;
using System.Linq;
using RenPyTRLauncher.Data;
using RenPyTRLauncher.Models;

namespace RenPyTRLauncher.Services
{
    public class EfHelpService : IHelpService
    {
        private readonly AppDbContext _db;
        public EfHelpService(AppDbContext db) => _db = db;

        public IEnumerable<HelpGuide> GetAll() =>
            _db.HelpGuides.OrderBy(h => h.Type).ThenBy(h => h.SortOrder).ToList();

        public IEnumerable<HelpGuide> GetActive() =>
            GetAll().Where(h => h.IsActive);

        public IEnumerable<HelpGuide> GetByType(HelpGuideType type) =>
            GetActive().Where(h => h.Type == type);

        public HelpGuide? GetById(Guid id) => _db.HelpGuides.Find(id);

        public void Add(HelpGuide guide)
        {
            _db.HelpGuides.Add(guide);
            _db.SaveChanges();
        }

        public void Update(HelpGuide guide)
        {
            _db.HelpGuides.Update(guide);
            _db.SaveChanges();
        }

        public void Remove(Guid id)
        {
            var ex = _db.HelpGuides.Find(id);
            if (ex != null)
            {
                _db.HelpGuides.Remove(ex);
                _db.SaveChanges();
            }
        }
    }
}
