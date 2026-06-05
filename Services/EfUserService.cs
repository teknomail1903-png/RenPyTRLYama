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
            return _db.Users.FirstOrDefault(u => u.Username == username);
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
                u.Role = "Admin";
                _db.SaveChanges();
            }
        }

        public void RevokeAdmin(Guid userId)
        {
            var u = GetById(userId);
            if (u != null)
            {
                u.Role = "User";
                _db.SaveChanges();
            }
        }
    }
}
