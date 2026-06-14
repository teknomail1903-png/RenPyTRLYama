namespace RenPyTRLauncher.Services
{
    public static class ServiceLocator
    {
        public static IGameService? GameService { get; set; }
        public static IAnnouncementService? AnnouncementService { get; set; }
        public static IUserService? UserService { get; set; }
        public static IPatchService? PatchService { get; set; }
        public static ISettingsService? SettingsService { get; set; }
        public static IMembershipService? MembershipService { get; set; }
        public static IActivityService? ActivityService { get; set; }
        public static ISupportService? SupportService { get; set; }
        public static IAuthService? AuthService { get; set; }
        public static ICategoryService? CategoryService { get; set; }
        public static INotificationService? NotificationService { get; set; }
        public static IHelpService? HelpService { get; set; }
        public static IDownloadService? DownloadService { get; set; }
        public static IGameTagService? GameTagService { get; set; }
        public static event Action? DataChanged;
        public static void NotifyDataChanged() => DataChanged?.Invoke();
    }
}
