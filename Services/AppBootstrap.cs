using RenPyTRLauncher.Data;

namespace RenPyTRLauncher.Services
{
    public static class AppBootstrap
    {
        public static AppDbContext InitializeServices()
        {
            var db = DatabaseInitializer.Initialize();

            ServiceLocator.GameService = new EfGameService(db);
            ServiceLocator.AnnouncementService = new EfAnnouncementService(db);
            ServiceLocator.UserService = new EfUserService(db);
            ServiceLocator.SettingsService = new EfSettingsService(db);
            ServiceLocator.MembershipService = new EfMembershipService(db);
            ServiceLocator.ActivityService = new EfActivityService(db);
            ServiceLocator.SupportService = new EfSupportService(db);
            ServiceLocator.AuthService = new AuthService(ServiceLocator.UserService);
            ServiceLocator.CategoryService = new EfCategoryService(db);
            ServiceLocator.NotificationService = new EfNotificationService(db, ServiceLocator.UserService);
            ServiceLocator.HelpService = new EfHelpService(db);
            ServiceLocator.PatchService = new PatchService(
                ServiceLocator.GameService,
                ServiceLocator.UserService);
            ServiceLocator.DownloadService = new DownloadService();
            ServiceLocator.GameTagService = new EfGameTagService(db);

            return db;
        }
    }
}
