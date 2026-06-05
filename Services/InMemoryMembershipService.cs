using System;
using System.Collections.Generic;
using System.Linq;
using RenPyTRLauncher.Models;

namespace RenPyTRLauncher.Services
{
    public class InMemoryMembershipService : IMembershipService
    {
        private readonly List<MembershipTier> _tiers = new();

        public IEnumerable<MembershipTier> GetAll() =>
            _tiers.Where(t => t.IsActive).OrderBy(t => t.SortOrder);

        public MembershipTier? GetById(Guid id) => _tiers.FirstOrDefault(t => t.Id == id);

        public void Add(MembershipTier tier) => _tiers.Add(tier);

        public void Update(MembershipTier tier)
        {
            var idx = _tiers.FindIndex(t => t.Id == tier.Id);
            if (idx >= 0) _tiers[idx] = tier;
        }

        public void Remove(Guid id) => _tiers.RemoveAll(t => t.Id == id);
    }
}
