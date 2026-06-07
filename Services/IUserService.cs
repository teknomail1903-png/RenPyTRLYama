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
        User? GetByEmail(string email);
        void Create(User user);
        void MakeMod(Guid userId);
        void RevokeMod(Guid userId);
        void Update(User user);
        void Delete(Guid id);
        void GrantVip(Guid userId, DateTime until);
        void RevokeVip(Guid userId);
        void MakeAdmin(Guid userId);
        void RevokeAdmin(Guid userId);
        void RecordPatchDownload(Guid userId, Guid gameId);
        void ToggleFavorite(Guid userId, Guid gameId);
    }
}
