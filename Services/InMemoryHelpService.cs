using System;
using System.Collections.Generic;
using System.Linq;
using RenPyTRLauncher.Models;

namespace RenPyTRLauncher.Services
{
    public class InMemoryHelpService : IHelpService
    {
        private readonly List<HelpGuide> _guides = new()
        {
            new()
            {
                Title = "Yama Nasıl Kurulur?",
                Content = "1. Oyunlar sayfasından oyunu seçin\n2. 'Yamayı Kur' butonuna tıklayın\n3. Oyun klasörünü seçin (game/ klasörünün üst dizini)\n4. Kurulum tamamlanana kadar bekleyin",
                Type = HelpGuideType.Text,
                SortOrder = 1
            },
            new()
            {
                Title = "VIP Üyelik Nedir?",
                Content = "VIP üyeler özel yamalara erken erişim kazanır ve öncelikli destek alır.",
                Type = HelpGuideType.FAQ,
                SortOrder = 1
            }
        };

        public IEnumerable<HelpGuide> GetAll() => _guides.OrderBy(h => h.Type).ThenBy(h => h.SortOrder);
        public IEnumerable<HelpGuide> GetActive() => GetAll().Where(h => h.IsActive);
        public IEnumerable<HelpGuide> GetByType(HelpGuideType type) => GetActive().Where(h => h.Type == type);
        public HelpGuide? GetById(Guid id) => _guides.FirstOrDefault(g => g.Id == id);
        public void Add(HelpGuide guide) => _guides.Add(guide);
        public void Update(HelpGuide guide) { }
        public void Remove(Guid id) => _guides.RemoveAll(g => g.Id == id);
    }
}
