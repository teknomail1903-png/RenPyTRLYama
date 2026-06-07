using RenPyTRLauncher.Models;

namespace RenPyTRLauncher.Services
{
    public interface IAuthService
    {
        AuthResult Register(string username, string email, string password);
        AuthResult Login(string usernameOrEmail, string password, bool rememberMe);
        AuthResult ChangePassword(Guid userId, string currentPassword, string newPassword);
        User? TryRestoreSession();
        void Logout();
        User? CurrentUser { get; }
    }
}
