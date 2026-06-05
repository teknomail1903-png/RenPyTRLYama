using System;
using System.Collections.Generic;
using RenPyTRLauncher.Models;

namespace RenPyTRLauncher.Services
{
    public interface IGameService
    {
        IEnumerable<Game> GetAll();
        Game? GetById(Guid id);
        void Add(Game g);
        void Update(Game g);
        void Remove(Guid id);
    }
}
