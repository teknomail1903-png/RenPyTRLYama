using System;
using System.Collections.Generic;
using System.Linq;
using RenPyTRLauncher.Models;

namespace RenPyTRLauncher.Services
{
    public class InMemoryCategoryService : ICategoryService
    {
        private readonly List<GameCategory> _categories = new()
        {
            new() { Name = "Devam Eden", DisplayName = "Devam Edenler", Icon = "📁", AccentColor = "#3498DB", SortOrder = 1 },
            new() { Name = "Biten", DisplayName = "Bitenler", Icon = "✅", AccentColor = "#27AE60", SortOrder = 2 },
            new() { Name = "Devam Etmeyen", DisplayName = "Devam Etmeyenler", Icon = "⏸", AccentColor = "#E67E22", SortOrder = 3 },
            new() { Name = "Erkek Başrol", DisplayName = "Erkek Başrol", Icon = "👨", AccentColor = "#9B59B6", SortOrder = 4 },
            new() { Name = "Kadın Başrol", DisplayName = "Kadın Başrol", Icon = "👩", AccentColor = "#E91E63", SortOrder = 5 },
            new() { Name = "VIP", DisplayName = "VIP", Icon = "💎", AccentColor = "#F1C40F", SortOrder = 6 },
            new() { Name = "Romance", DisplayName = "Romance", Icon = "💕", AccentColor = "#E74C3C", SortOrder = 7 }
        };

        public IEnumerable<GameCategory> GetAll() => _categories.OrderBy(c => c.SortOrder);
        public IEnumerable<GameCategory> GetActive() => GetAll().Where(c => c.IsActive);
        public GameCategory? GetById(Guid id) => _categories.FirstOrDefault(c => c.Id == id);
        public GameCategory? GetByName(string name) =>
            _categories.FirstOrDefault(c => c.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

        public void Add(GameCategory category) => _categories.Add(category);
        public void Update(GameCategory category) { }
        public void Remove(Guid id) => _categories.RemoveAll(c => c.Id == id);
        public string[] GetCategoryNames() => GetActive().Select(c => c.Name).ToArray();
    }
}
