using System;
using System.Collections.Generic;
using System.Linq;
using RenPyTRLauncher.Data;
using RenPyTRLauncher.Models;

namespace RenPyTRLauncher.Services
{
    public class EfCategoryService : ICategoryService
    {
        private readonly AppDbContext _db;
        public EfCategoryService(AppDbContext db) => _db = db;

        public IEnumerable<GameCategory> GetAll() =>
            _db.Categories.OrderBy(c => c.SortOrder).ThenBy(c => c.DisplayName).ToList();

        public IEnumerable<GameCategory> GetActive() =>
            GetAll().Where(c => c.IsActive);

        public GameCategory? GetById(Guid id) => _db.Categories.Find(id);

        public GameCategory? GetByName(string name) =>
            _db.Categories.FirstOrDefault(c =>
                c.Name.ToLower() == name.ToLower());

        public void Add(GameCategory category)
        {
            _db.Categories.Add(category);
            _db.SaveChanges();
        }

        public void Update(GameCategory category)
        {
            _db.Categories.Update(category);
            _db.SaveChanges();
        }

        public void Remove(Guid id)
        {
            var ex = _db.Categories.Find(id);
            if (ex != null)
            {
                _db.Categories.Remove(ex);
                _db.SaveChanges();
            }
        }

        public string[] GetCategoryNames() => GetActive().Select(c => c.Name).ToArray();
    }
}
