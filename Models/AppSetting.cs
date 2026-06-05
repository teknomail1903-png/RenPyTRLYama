using System;

namespace RenPyTRLauncher.Models
{
    public class AppSetting
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Key { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
    }

    public static class AppSettingKeys
    {
        public const string WebsiteUrl = "WebsiteUrl";
        public const string DiscordUrl = "DiscordUrl";
        public const string AnnouncementsUrl = "AnnouncementsUrl";
        public const string SupportUrl = "SupportUrl";
    }
}
