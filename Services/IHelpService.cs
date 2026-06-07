using System;
using System.Collections.Generic;
using RenPyTRLauncher.Models;

namespace RenPyTRLauncher.Services
{
    public interface IHelpService
    {
        IEnumerable<HelpGuide> GetAll();
        IEnumerable<HelpGuide> GetActive();
        IEnumerable<HelpGuide> GetByType(HelpGuideType type);
        HelpGuide? GetById(Guid id);
        void Add(HelpGuide guide);
        void Update(HelpGuide guide);
        void Remove(Guid id);
    }
}
