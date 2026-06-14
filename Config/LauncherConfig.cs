namespace RenPyTRLauncher.Config
{
    public static class LauncherConfig
    {
        // Discord Link
        public static string DiscordUrl { get; set; } = "https://discord.gg/placeholder";
        
        // VIP Market Link
        public static string VipMarketUrl { get; set; } = "https://placeholder.com/vip";
        
        // News/Announcements
        public static string NewsText { get; set; } = "Yeni oyunlar eklendi! VIP üyeliğe özel içerikler...";
        
        // Banner Configuration
        public static bool UseRealBannerImages { get; set; } = true;
        
        // Load from config file (future implementation)
        public static void LoadConfig()
        {
            // Future: Load from appsettings.json or config file
            // For now, using default values
        }
        
        // Save to config file (future implementation for admin panel)
        public static void SaveConfig()
        {
            // Future: Save to appsettings.json or config file
            // For admin panel management
        }
    }
}
