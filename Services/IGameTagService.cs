using System;
using System.Collections.Generic;
using RenPyTRLauncher.Models;

namespace RenPyTRLauncher.Services
{
    public interface IGameTagService
    {
        List<GameTag> GetTagsByGameId(Guid gameId);
        List<GameTag> GetAllTags();
        List<string> GetTagNamesByGameId(Guid gameId);
        void AddTag(GameTag tag);
        void RemoveTag(int tagId);
        void RemoveTagsByGameId(Guid gameId);
        List<Game> GetGamesByTagName(string tagName);
        bool TagExists(Guid gameId, string tagName);
    }
}
