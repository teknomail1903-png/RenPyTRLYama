using System;
using System.Collections.Generic;
using RenPyTRLauncher.Models;

namespace RenPyTRLauncher.Services
{
    public interface IActivityService
    {
        IEnumerable<UserActivity> GetForUser(Guid userId, int limit = 20);
        void Log(Guid userId, string description, string icon = "🎮");
    }
}
