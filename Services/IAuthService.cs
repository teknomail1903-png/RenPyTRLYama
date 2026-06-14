using RenPyTRLauncher.Models;

namespace RenPyTRLauncher.Services
{
    public interface IAuthService
    {
        AuthResult Register(string username, string email, string password, string secretQuestion = "", string secretAnswer = "");
        AuthResult Login(string usernameOrEmail, string password, bool rememberMe);
        AuthResult ChangePassword(Guid userId, string currentPassword, string newPassword);
        AuthResult ResetPassword(Guid userId, string newPassword);
        User? TryRestoreSession();
        void Logout();
        User? CurrentUser { get; }
    }
}
