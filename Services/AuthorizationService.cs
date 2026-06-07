using RenPyTRLauncher.Models;

namespace RenPyTRLauncher.Services
{
    public static class AuthorizationService
    {
        public static bool CanAccessAdminPanel(User? user) =>
            user != null && UserRole.IsMod(user.Role);

        public static bool CanManageGames(User? user) =>
            user != null && UserRole.IsAdmin(user.Role);

        public static bool CanManageUsers(User? user) =>
            user != null && UserRole.IsAdmin(user.Role);

        public static bool CanManageSupport(User? user) =>
            user != null && UserRole.IsMod(user.Role);

        public static bool CanManageAnnouncements(User? user) =>
            user != null && UserRole.IsAdmin(user.Role);

        public static bool CanManageSettings(User? user) =>
            user != null && UserRole.IsAdmin(user.Role);

        public static bool CanAccessVipContent(User? user) =>
            user != null && user.IsVip;

        public static bool CanInstallVipPatch(User? user, Game game) =>
            !game.IsVip || CanAccessVipContent(user);

        public static void SyncVipBadges(User user)
        {
            if (user.IsVip)
            {
                if (!user.Badges.Contains("💎 VIP Üye"))
                    user.Badges.Add("💎 VIP Üye");
            }
            else
            {
                user.Badges.RemoveAll(b => b.Contains("VIP"));
            }

            if (UserRole.IsAdmin(user.Role) && !user.Badges.Contains("🛡️ Admin"))
                user.Badges.Add("🛡️ Admin");
            else if (!UserRole.IsAdmin(user.Role))
                user.Badges.RemoveAll(b => b.Contains("Admin"));

            if (UserRole.IsMod(user.Role) && !UserRole.IsAdmin(user.Role) && !user.Badges.Contains("🔧 Moderatör"))
                user.Badges.Add("🔧 Moderatör");
            else if (!UserRole.IsMod(user.Role))
                user.Badges.RemoveAll(b => b.Contains("Moderatör"));
        }
    }
}
