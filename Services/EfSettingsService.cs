using System.Collections.Generic;
using System.Linq;
using RenPyTRLauncher.Data;
using RenPyTRLauncher.Models;

namespace RenPyTRLauncher.Services
{
    public class EfSettingsService : ISettingsService
    {
        private readonly AppDbContext _db;

        public EfSettingsService(AppDbContext db) => _db = db;

        public string Get(string key, string defaultValue = "")
        {
            var s = _db.AppSettings.FirstOrDefault(x => x.Key == key);
            return s?.Value ?? defaultValue;
        }

        public void Set(string key, string value)
        {
            var s = _db.AppSettings.FirstOrDefault(x => x.Key == key);
            if (s == null)
            {
                _db.AppSettings.Add(new AppSetting { Key = key, Value = value });
            }
            else
            {
                s.Value = value;
            }
            _db.SaveChanges();
        }

        public Dictionary<string, string> GetAll()
        {
            return _db.AppSettings.ToDictionary(x => x.Key, x => x.Value);
        }

        public void SaveAll(Dictionary<string, string> settings)
        {
            foreach (var kv in settings)
                Set(kv.Key, kv.Value);
        }
    }
}
