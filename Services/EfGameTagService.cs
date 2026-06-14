using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using RenPyTRLauncher.Data;
using RenPyTRLauncher.Models;

namespace RenPyTRLauncher.Services
{
    public class EfGameTagService : IGameTagService
    {
        private readonly AppDbContext _db;

        public EfGameTagService(AppDbContext db)
        {
            _db = db;
        }

        public List<GameTag> GetTagsByGameId(Guid gameId)
        {
            return _db.GameTags.Where(t => t.GameId == gameId).ToList();
        }

        public List<GameTag> GetAllTags()
        {
            return _db.GameTags.ToList();
        }

        public List<string> GetTagNamesByGameId(Guid gameId)
        {
            return _db.GameTags.Where(t => t.GameId == gameId).Select(t => t.Name).ToList();
        }

        public void AddTag(GameTag tag)
        {
            tag.CreatedAt = DateTime.UtcNow;
            _db.GameTags.Add(tag);
            _db.SaveChanges();
        }

        public void RemoveTag(int tagId)
        {
            var tag = _db.GameTags.FirstOrDefault(t => t.Id == tagId);
            if (tag != null)
            {
                _db.GameTags.Remove(tag);
                _db.SaveChanges();
            }
        }

        public void RemoveTagsByGameId(Guid gameId)
        {
            var tags = _db.GameTags.Where(t => t.GameId == gameId).ToList();
            _db.GameTags.RemoveRange(tags);
            _db.SaveChanges();
        }

        public List<Game> GetGamesByTagName(string tagName)
        {
            var gameIds = _db.GameTags.Where(t => t.Name == tagName).Select(t => t.GameId).ToList();
            return _db.Games.Where(g => gameIds.Contains(g.Id)).ToList();
        }

        public bool TagExists(Guid gameId, string tagName)
        {
            return _db.GameTags.Any(t => t.GameId == gameId && t.Name == tagName);
        }
    }
}
