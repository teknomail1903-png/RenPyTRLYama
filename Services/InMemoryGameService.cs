using System;
using System.Collections.Generic;
using System.Linq;
using RenPyTRLauncher.Models;

namespace RenPyTRLauncher.Services
{
    public class InMemoryGameService : IGameService
    {
        private readonly List<Game> _games = new();

        public InMemoryGameService()
        {
            _games.Add(new Game { Id = Data.DbSeeder.SummerMemoriesId, Name = "Summer Memories", ImagePath = "Images/summermemories.jpg", Categories = new System.Collections.Generic.List<string> { "VIP", "Romance", "Erkek Başrol" }, IsVip = true, IsFeatured = true, IsTop10 = true, DownloadCount = 1200, PatchVersion = "v1.0" });
            _games.Add(new Game { Id = Data.DbSeeder.BeingADikId, Name = "Being A DIK", ImagePath = "Images/beingadik.jpg", Categories = new System.Collections.Generic.List<string> { "Devam Eden", "Erkek Başrol" }, IsTop10 = true, DownloadCount = 800, PatchVersion = "v0.9.4-tr" });
            _games.Add(new Game { Id = Data.DbSeeder.MilfyCityId, Name = "Milfy City", ImagePath = "Images/milfycity.jpg", Categories = new System.Collections.Generic.List<string> { "Biten", "Kadın Başrol" }, DownloadCount = 600, PatchVersion = "v1.0-tr" });
        }

        public IEnumerable<Game> GetAll() => _games.OrderByDescending(g => g.CreatedDate);
        public Game? GetById(Guid id) => _games.FirstOrDefault(g => g.Id == id);
        public void Add(Game g) => _games.Add(g);
        public void Update(Game g)
        {
            var ex = GetById(g.Id);
            if (ex == null) return;
            ex.Name = g.Name;
            ex.Description = g.Description;
            ex.ImagePath = g.ImagePath;
            ex.Categories = g.Categories;
            ex.IsVip = g.IsVip;
            ex.IsTop10 = g.IsTop10;
            ex.IsFeatured = g.IsFeatured;
            ex.Version = g.Version;
            ex.PatchFilePath = g.PatchFilePath;
            ex.PatchVersion = g.PatchVersion;
        }

        public void Remove(Guid id) => _games.RemoveAll(g => g.Id == id);

        public void IncrementDownloadCount(Guid gameId)
        {
            var game = GetById(gameId);
            if (game != null) game.DownloadCount++;
        }
    }
}
