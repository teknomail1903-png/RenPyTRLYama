using System;
using System.Collections.Generic;
using RenPyTRLauncher.Models;

namespace RenPyTRLauncher.Services
{
    public interface IUserService
    {
        IEnumerable<User> GetAll();
        User? GetById(Guid id);
        User? GetByUsername(string username);
        void Create(User user);
        void Update(User user);
        void Delete(Guid id);
        void GrantVip(Guid userId, DateTime until);
        void RevokeVip(Guid userId);
        void MakeAdmin(Guid userId);
        void RevokeAdmin(Guid userId);
    }
}
