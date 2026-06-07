using System;
using System.IO;
using System.Text.Json;
using RenPyTRLauncher.Models;

namespace RenPyTRLauncher.Services
{
    public class SessionData
    {
        public Guid UserId { get; set; }
        public string Token { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
        public bool RememberMe { get; set; }
    }

    public static class SessionService
    {
        private static readonly string SessionDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "RenPyTRLauncher");

        private static readonly string SessionFile = Path.Combine(SessionDir, "session.json");

        public static SessionData? Current { get; private set; }

        public static void Save(Guid userId, bool rememberMe)
        {
            var session = new SessionData
            {
                UserId = userId,
                Token = Guid.NewGuid().ToString("N"),
                ExpiresAt = rememberMe ? DateTime.UtcNow.AddDays(30) : DateTime.UtcNow.AddHours(12),
                RememberMe = rememberMe
            };
            Current = session;
            Directory.CreateDirectory(SessionDir);
            File.WriteAllText(SessionFile, JsonSerializer.Serialize(session));
        }

        public static SessionData? Load()
        {
            try
            {
                if (!File.Exists(SessionFile)) return null;
                var json = File.ReadAllText(SessionFile);
                var session = JsonSerializer.Deserialize<SessionData>(json);
                if (session == null || session.ExpiresAt < DateTime.UtcNow) return null;
                Current = session;
                return session;
            }
            catch
            {
                return null;
            }
        }

        public static void Clear()
        {
            Current = null;
            try
            {
                if (File.Exists(SessionFile)) File.Delete(SessionFile);
            }
            catch { }
        }

        public static bool IsSessionValid(User? user)
        {
            if (Current == null || user == null) return false;
            return Current.UserId == user.Id && Current.ExpiresAt >= DateTime.UtcNow;
        }
    }
}
