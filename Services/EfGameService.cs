using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RenPyTRLauncher.Data;
using RenPyTRLauncher.Models;

namespace RenPyTRLauncher.Services
{
    public class EfGameService : IGameService
    {
        private readonly AppDbContext _db;
        public EfGameService(AppDbContext db) { _db = db; }

        public IEnumerable<Game> GetAll() => _db.Games.OrderByDescending(g => g.CreatedDate).ToList();
        public Game? GetById(Guid id) => _db.Games.FirstOrDefault(g => g.Id == id);
        public void Add(Game g) { _db.Games.Add(g); _db.SaveChanges(); }
        public void Update(Game g) { _db.Games.Update(g); _db.SaveChanges(); }
        public void Remove(Guid id) { var ex = _db.Games.Find(id); if (ex != null) { _db.Games.Remove(ex); _db.SaveChanges(); } }
    }
}
