using System;
using System.Collections.Generic;
using System.Linq;
using RenPyTRLauncher.Data;
using RenPyTRLauncher.Models;

namespace RenPyTRLauncher.Services
{
    public class EfUserService : IUserService
    {
        private readonly AppDbContext _db;

        public EfUserService(AppDbContext db)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
        }

        public IEnumerable<User> GetAll()
        {
            return _db.Users.ToList();
        }

        public User? GetById(Guid id)
        {
            return _db.Users.Find(id);
        }

        public User? GetByUsername(string username)
        {
            return _db.Users.FirstOrDefault(u =>
                u.Username.ToLower() == username.ToLower());
        }

        public User? GetByEmail(string email)
        {
            return _db.Users.FirstOrDefault(u =>
                u.Email.ToLower() == email.ToLower());
        }

        public void Create(User user)
        {
            _db.Users.Add(user);
            _db.SaveChanges();
        }

        public void Update(User user)
        {
            _db.Users.Update(user);
            _db.SaveChanges();
        }

        public void Delete(Guid id)
        {
            var ex = _db.Users.Find(id);
            if (ex != null)
            {
                _db.Users.Remove(ex);
                _db.SaveChanges();
            }
        }

        public void GrantVip(Guid userId, DateTime until)
        {
            var u = GetById(userId);
            if (u != null)
            {
                u.IsVip = true;
                u.VipEndDate = until;
                _db.SaveChanges();
            }
        }

        public void RevokeVip(Guid userId)
        {
            var u = GetById(userId);
            if (u != null)
            {
                u.IsVip = false;
                u.VipEndDate = null;
                _db.SaveChanges();
            }
        }

        public void MakeAdmin(Guid userId)
        {
            var u = GetById(userId);
            if (u != null)
            {
                u.Role = UserRole.Admin;
                _db.SaveChanges();
            }
        }

        public void RevokeAdmin(Guid userId)
        {
            var u = GetById(userId);
            if (u != null)
            {
                u.Role = UserRole.User;
                _db.SaveChanges();
            }
        }

        public void MakeMod(Guid userId)
        {
            var u = GetById(userId);
            if (u != null)
            {
                u.Role = UserRole.Mod;
                _db.SaveChanges();
            }
        }

        public void RevokeMod(Guid userId)
        {
            var u = GetById(userId);
            if (u != null)
            {
                u.Role = UserRole.User;
                _db.SaveChanges();
            }
        }

        public void RecordPatchDownload(Guid userId, Guid gameId)
        {
            var u = GetById(userId);
            if (u == null) return;

            if (!u.DownloadedPatchIds.Contains(gameId))
                u.DownloadedPatchIds.Add(gameId);

            u.RecentDownloadedGameIds.Remove(gameId);
            u.RecentDownloadedGameIds.Insert(0, gameId);
            if (u.RecentDownloadedGameIds.Count > 20)
                u.RecentDownloadedGameIds = u.RecentDownloadedGameIds.Take(20).ToList();

            u.TotalDownloadCount++;
            _db.SaveChanges();
        }

        public void ToggleFavorite(Guid userId, Guid gameId)
        {
            var u = GetById(userId);
            if (u == null) return;

            if (u.FavoriteGameIds.Contains(gameId))
                u.FavoriteGameIds.Remove(gameId);
            else
                u.FavoriteGameIds.Add(gameId);

            _db.SaveChanges();
        }
    }
}
