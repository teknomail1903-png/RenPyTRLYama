using System;
using System.Collections.Generic;
using System.Linq;
using RenPyTRLauncher.Data;
using RenPyTRLauncher.Models;

namespace RenPyTRLauncher.Services
{
    public class EfActivityService : IActivityService
    {
        private readonly AppDbContext _db;

        public EfActivityService(AppDbContext db) => _db = db;

        public IEnumerable<UserActivity> GetForUser(Guid userId, int limit = 20)
        {
            return _db.UserActivities
                .Where(a => a.UserId == userId)
                .OrderByDescending(a => a.OccurredAt)
                .Take(limit)
                .ToList();
        }

        public void Log(Guid userId, string description, string icon = "🎮")
        {
            _db.UserActivities.Add(new UserActivity
            {
                UserId = userId,
                Description = description,
                Icon = icon,
                OccurredAt = DateTime.UtcNow
            });
            _db.SaveChanges();
        }
    }
}
