using System;
using System.Collections.Generic;
using System.Linq;
using RenPyTRLauncher.Data;
using RenPyTRLauncher.Models;

namespace RenPyTRLauncher.Services
{
    public class EfMembershipService : IMembershipService
    {
        private readonly AppDbContext _db;

        public EfMembershipService(AppDbContext db) => _db = db;

        public IEnumerable<MembershipTier> GetAll()
        {
            return _db.MembershipTiers
                .Where(t => t.IsActive)
                .OrderBy(t => t.SortOrder)
                .ToList();
        }

        public MembershipTier? GetById(Guid id) => _db.MembershipTiers.Find(id);

        public void Add(MembershipTier tier)
        {
            _db.MembershipTiers.Add(tier);
            _db.SaveChanges();
        }

        public void Update(MembershipTier tier)
        {
            _db.MembershipTiers.Update(tier);
            _db.SaveChanges();
        }

        public void Remove(Guid id)
        {
            var t = _db.MembershipTiers.Find(id);
            if (t != null)
            {
                _db.MembershipTiers.Remove(t);
                _db.SaveChanges();
            }
        }
    }
}
