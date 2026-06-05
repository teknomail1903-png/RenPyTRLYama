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
        public static event Action? DataChanged;
        public static void NotifyDataChanged() => DataChanged?.Invoke();
    }
}
