using System.Collections.Generic;
using RenPyTRLauncher.Models;

namespace RenPyTRLauncher.Services
{
    public interface ISettingsService
    {
        string Get(string key, string defaultValue = "");
        void Set(string key, string value);
        Dictionary<string, string> GetAll();
        void SaveAll(Dictionary<string, string> settings);
    }
}
