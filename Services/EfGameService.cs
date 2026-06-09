using System;
using System.Collections.Generic;
using System.Linq;
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

        public void Add(Game g)
        {
            System.Diagnostics.Debug.WriteLine($"[TEST LOG] Oyun/Yama Ekleme Başladı - Type: {g.Type}, Name: {g.Name}");
            
            // Generate new Guid if Id is empty
            if (g.Id == Guid.Empty)
            {
                g.Id = Guid.NewGuid();
                System.Diagnostics.Debug.WriteLine($"EfGameService.Add - Generated new Guid for game: {g.Id}");
            }

            g.CreatedDate = DateTime.UtcNow;
            g.UpdatedDate = DateTime.UtcNow;

            // Check if entity is already tracked
            var existing = _db.Games.Local.FirstOrDefault(e => e.Id == g.Id);
            if (existing != null)
            {
                _db.Entry(existing).State = EntityState.Detached;
            }

            try
            {
                _db.Games.Add(g);
                _db.SaveChanges();
                System.Diagnostics.Debug.WriteLine($"[TEST LOG] ✅ BAŞARILI: {g.Type} eklendi - ID: {g.Id}, Name: {g.Name}");
            }
            catch (DbUpdateConcurrencyException ex)
            {
                System.Diagnostics.Debug.WriteLine($"[TEST LOG] ❌ HATA: DbUpdateConcurrencyException - Game.Id={g.Id}, Game.Name={g.Name}");
                System.Diagnostics.Debug.WriteLine($"Exception: {ex.Message}");
                throw new Exception($"Kayıt eklenirken eşzamanlılık hatası oluştu. Game.Id={g.Id}, Game.Name={g.Name}", ex);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[TEST LOG] ❌ HATA: {ex.Message} - Game.Id={g.Id}, Game.Name={g.Name}");
                throw;
            }
        }

        public void Update(Game g)
        {
            System.Diagnostics.Debug.WriteLine($"[TEST LOG] Oyun/Yama Düzenleme Başladı - Type: {g.Type}, ID: {g.Id}, Name: {g.Name}");
            
            g.UpdatedDate = DateTime.UtcNow;

            // Log all games in database for debugging
            var allGames = _db.Games.ToList();
            System.Diagnostics.Debug.WriteLine($"=== ALL GAMES IN DATABASE ===");
            foreach (var game in allGames)
            {
                System.Diagnostics.Debug.WriteLine($"DB Game.Id={game.Id}, Game.Name={game.Name}");
            }
            System.Diagnostics.Debug.WriteLine($"=== END ALL GAMES ===");

            // Fetch existing game from database
            var existingGame = _db.Games.FirstOrDefault(x => x.Id == g.Id);

            // Log details before update
            System.Diagnostics.Debug.WriteLine($"Update attempt - Game.Id={g.Id}, Game.Name={g.Name}");
            System.Diagnostics.Debug.WriteLine($"Existing game in DB: {existingGame != null}");
            System.Diagnostics.Debug.WriteLine($"Entity state: {_db.Entry(g).State}");
            System.Diagnostics.Debug.WriteLine($"Object reference equality: {existingGame == g}");

            if (existingGame == null)
            {
                System.Diagnostics.Debug.WriteLine($"[TEST LOG] ❌ HATA: Kayıt bulunamadı veya silinmiş - Game.Id={g.Id}");
                throw new Exception($"Kayıt bulunamadı veya silinmiş. Game.Id={g.Id}, Game.Name={g.Name}");
            }

            // Update fields one by one instead of using Update()
            existingGame.Name = g.Name;
            existingGame.Description = g.Description;
            existingGame.Version = g.Version;
            existingGame.ImagePath = g.ImagePath;
            existingGame.Categories = g.Categories;
            existingGame.IsVip = g.IsVip;
            existingGame.IsTop10 = g.IsTop10;
            existingGame.IsFeatured = g.IsFeatured;
            existingGame.UpdatedDate = DateTime.UtcNow;
            existingGame.PatchFilePath = g.PatchFilePath;
            existingGame.PatchVersion = g.PatchVersion;
            existingGame.ScreenshotPaths = g.ScreenshotPaths;
            existingGame.PatchNotes = g.PatchNotes;
            existingGame.DownloadLinks = g.DownloadLinks;
            existingGame.Type = g.Type;
            existingGame.TurkishStatus = g.TurkishStatus;
            existingGame.SteamStatus = g.SteamStatus;
            existingGame.UpdateStatus = g.UpdateStatus;
            existingGame.ParentGameId = g.ParentGameId;

            try
            {
                var affectedRows = _db.SaveChanges();
                System.Diagnostics.Debug.WriteLine($"[TEST LOG] ✅ BAŞARILI: {g.Type} düzenlendi - ID: {g.Id}, Name: {g.Name}, Affected rows: {affectedRows}");
            }
            catch (DbUpdateConcurrencyException ex)
            {
                System.Diagnostics.Debug.WriteLine($"[TEST LOG] ❌ HATA: DbUpdateConcurrencyException - Game.Id={g.Id}, Game.Name={g.Name}");
                System.Diagnostics.Debug.WriteLine($"Exception: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Expected 1 row affected, but 0 rows were affected");
                throw new Exception($"Kayıt güncellenirken eşzamanlılık hatası oluştu. Game.Id={g.Id}, Game.Name={g.Name}. Beklenen 1 satır, ancak 0 satır etkilendi.", ex);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[TEST LOG] ❌ HATA: {ex.Message} - Game.Id={g.Id}, Game.Name={g.Name}");
                throw;
            }
        }

        public void Remove(Guid id)
        {
            System.Diagnostics.Debug.WriteLine($"[TEST LOG] Oyun/Yama Silme Başladı - ID: {id}");
            
            var ex = _db.Games.Find(id);
            if (ex != null)
            {
                var gameName = ex.Name;
                var gameType = ex.Type;
                _db.Games.Remove(ex);
                _db.SaveChanges();
                System.Diagnostics.Debug.WriteLine($"[TEST LOG] ✅ BAŞARILI: {gameType} silindi - ID: {id}, Name: {gameName}");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"[TEST LOG] ❌ HATA: Kayıt bulunamadı - ID: {id}");
            }
        }

        public void IncrementDownloadCount(Guid gameId)
        {
            var game = _db.Games.Find(gameId);
            if (game == null) return;
            game.DownloadCount++;
            _db.SaveChanges();
        }
    }
}
