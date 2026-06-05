using System;
using System.Collections.Generic;
using RenPyTRLauncher.Models;

namespace RenPyTRLauncher.Services
{
    public interface IMembershipService
    {
        IEnumerable<MembershipTier> GetAll();
        MembershipTier? GetById(Guid id);
        void Add(MembershipTier tier);
        void Update(MembershipTier tier);
        void Remove(Guid id);
    }
}
