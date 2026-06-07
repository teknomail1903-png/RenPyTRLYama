namespace RenPyTRLauncher.Models
{
    public static class UserRole
    {
        public const string User = "User";
        public const string Mod = "Mod";
        public const string Admin = "Admin";

        public static bool IsAdmin(string? role) =>
            string.Equals(role, Admin, StringComparison.OrdinalIgnoreCase);

        public static bool IsMod(string? role) =>
            string.Equals(role, Mod, StringComparison.OrdinalIgnoreCase) || IsAdmin(role);

        public static bool IsAtLeast(string? role, string minimum)
        {
            if (IsAdmin(role)) return true;
            if (string.Equals(minimum, Admin, StringComparison.OrdinalIgnoreCase)) return IsAdmin(role);
            if (string.Equals(minimum, Mod, StringComparison.OrdinalIgnoreCase)) return IsMod(role);
            return true;
        }
    }
}
