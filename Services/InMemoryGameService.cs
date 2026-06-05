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
            // örnek oyunlar
            _games.Add(new Game { Name = "Summer Memories", ImagePath = "Images/summermemories.jpg", Categories = new System.Collections.Generic.List<string>{ "VIP", "Romance" }, IsVip = true, IsFeatured = true, DownloadCount = 1200 });
            _games.Add(new Game { Name = "Being A DIK", ImagePath = "Images/beingadik.jpg", Categories = new System.Collections.Generic.List<string>{ "Devam Eden" }, DownloadCount = 800 });
            _games.Add(new Game { Name = "Milfy City", ImagePath = "Images/milfycity.jpg", Categories = new System.Collections.Generic.List<string>{ "Biten" }, DownloadCount = 600 });
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
    }
}
