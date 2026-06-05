using System.Collections.Generic;
using RenPyTRLauncher.Models;

namespace RenPyTRLauncher.Services
{
    public class InMemorySettingsService : ISettingsService
    {
        private readonly Dictionary<string, string> _store = new()
        {
            [AppSettingKeys.WebsiteUrl] = "https://renpytr.com",
            [AppSettingKeys.DiscordUrl] = "https://discord.gg/renpytr",
            [AppSettingKeys.AnnouncementsUrl] = "https://renpytr.com/duyurular",
            [AppSettingKeys.SupportUrl] = "https://renpytr.com/destek"
        };

        public string Get(string key, string defaultValue = "") =>
            _store.TryGetValue(key, out var v) ? v : defaultValue;

        public void Set(string key, string value) => _store[key] = value;

        public Dictionary<string, string> GetAll() => new(_store);

        public void SaveAll(Dictionary<string, string> settings)
        {
            foreach (var kv in settings) _store[kv.Key] = kv.Value;
        }
    }
}
