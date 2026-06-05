using System;
using System.Collections.Generic;
using System.Linq;
using RenPyTRLauncher.Models;

namespace RenPyTRLauncher.Services
{
    public class InMemoryActivityService : IActivityService
    {
        private readonly List<UserActivity> _activities = new();

        public IEnumerable<UserActivity> GetForUser(Guid userId, int limit = 20) =>
            _activities.Where(a => a.UserId == userId).OrderByDescending(a => a.OccurredAt).Take(limit);

        public void Log(Guid userId, string description, string icon = "🎮")
        {
            _activities.Add(new UserActivity
            {
                UserId = userId,
                Description = description,
                Icon = icon,
                OccurredAt = DateTime.UtcNow
            });
        }
    }
}
