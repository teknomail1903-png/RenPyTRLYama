using System;
using System.Collections.Generic;
using RenPyTRLauncher.Models;

namespace RenPyTRLauncher.Services
{
    public interface ICategoryService
    {
        IEnumerable<GameCategory> GetAll();
        IEnumerable<GameCategory> GetActive();
        GameCategory? GetById(Guid id);
        GameCategory? GetByName(string name);
        void Add(GameCategory category);
        void Update(GameCategory category);
        void Remove(Guid id);
        string[] GetCategoryNames();
    }
}
