using System;
using System.Collections.Generic;
using System.Linq;
using RenPyTRLauncher.Models;

namespace RenPyTRLauncher.Services
{
    public class InMemoryUserService : IUserService
    {
        private readonly List<User> _users = new();

        public InMemoryUserService()
        {
            // örnek kullanıcılar
            var u1 = new User { Username = "argion", Email = "argion@example.com", IsVip = true, VipEndDate = DateTime.UtcNow.AddDays(30), Role = "Admin" };
            u1.FavoriteGameIds.Add(Guid.NewGuid());
            u1.DownloadedPatchIds.Add(Guid.NewGuid());
            u1.RecentDownloadedGameIds.Add(Guid.NewGuid());
            u1.TotalDownloadCount = 5;

            var u2 = new User { Username = "user1", Email = "user1@example.com", IsVip = false, Role = "User" };
            u2.TotalDownloadCount = 0;

            _users.Add(u1);
            _users.Add(u2);
        }

        public IEnumerable<User> GetAll() => _users;
        public User? GetById(Guid id) => _users.FirstOrDefault(u => u.Id == id);
        public User? GetByUsername(string username) => _users.FirstOrDefault(u => u.Username.Equals(username, StringComparison.OrdinalIgnoreCase));
        public void Create(User user) => _users.Add(user);
        public void Update(User user)
        {
            var existing = GetById(user.Id);
            if (existing == null) return;
            existing.Username = user.Username;
            existing.Email = user.Email;
            existing.PasswordHash = user.PasswordHash;
            existing.IsVip = user.IsVip;
            existing.VipEndDate = user.VipEndDate;
            existing.Role = user.Role;
            // yeni alanlar
            existing.FavoriteGameIds = new System.Collections.Generic.List<Guid>(user.FavoriteGameIds ?? new System.Collections.Generic.List<Guid>());
            existing.DownloadedPatchIds = new System.Collections.Generic.List<Guid>(user.DownloadedPatchIds ?? new System.Collections.Generic.List<Guid>());
            existing.RecentDownloadedGameIds = new System.Collections.Generic.List<Guid>(user.RecentDownloadedGameIds ?? new System.Collections.Generic.List<Guid>());
            existing.TotalDownloadCount = user.TotalDownloadCount;
        }

        public void Delete(Guid id) => _users.RemoveAll(u => u.Id == id);

        public void GrantVip(Guid userId, DateTime until)
        {
            var u = GetById(userId);
            if (u == null) return;
            u.IsVip = true;
            u.VipEndDate = until;
        }

        public void RevokeVip(Guid userId)
        {
            var u = GetById(userId);
            if (u == null) return;
            u.IsVip = false;
            u.VipEndDate = null;
        }

        public void MakeAdmin(Guid userId)
        {
            var u = GetById(userId);
            if (u == null) return;
            u.Role = "Admin";
        }

        public void RevokeAdmin(Guid userId)
        {
            var u = GetById(userId);
            if (u == null) return;
            u.Role = "User";
        }
    }
}
