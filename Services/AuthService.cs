using System.Linq;
using RenPyTRLauncher.Models;

namespace RenPyTRLauncher.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserService _userService;

        public AuthService(IUserService userService)
        {
            _userService = userService;
        }

        public User? CurrentUser { get; private set; }

        public AuthResult Register(string username, string email, string password)
        {
            username = username?.Trim() ?? "";
            email = email?.Trim() ?? "";

            if (username.Length < 3)
                return AuthResult.Fail("Kullanıcı adı en az 3 karakter olmalıdır.");
            if (password.Length < 6)
                return AuthResult.Fail("Şifre en az 6 karakter olmalıdır.");
            if (!email.Contains('@'))
                return AuthResult.Fail("Geçerli bir e-posta adresi girin.");

            if (_userService.GetByUsername(username) != null)
                return AuthResult.Fail("Bu kullanıcı adı zaten kullanılıyor.");
            if (_userService.GetByEmail(email) != null)
                return AuthResult.Fail("Bu e-posta zaten kayıtlı.");

            var user = new User
            {
                Username = username,
                Email = email,
                PasswordHash = PasswordHasher.Hash(password),
                Role = UserRole.User,
                MembershipLevel = "Ücretsiz",
                AvatarPath = ImageService.GetDefaultAvatar(username)
            };
            AuthorizationService.SyncVipBadges(user);
            _userService.Create(user);

            CurrentUser = user;
            return AuthResult.Ok(user, "Kayıt başarılı. Hoş geldiniz!");
        }

        public AuthResult Login(string usernameOrEmail, string password, bool rememberMe)
        {
            usernameOrEmail = usernameOrEmail?.Trim() ?? "";
            if (string.IsNullOrEmpty(usernameOrEmail) || string.IsNullOrEmpty(password))
                return AuthResult.Fail("Kullanıcı adı ve şifre gereklidir.");

            var user = _userService.GetByUsername(usernameOrEmail)
                       ?? _userService.GetByEmail(usernameOrEmail);

            if (user == null || !PasswordHasher.Verify(password, user.PasswordHash))
                return AuthResult.Fail("Kullanıcı adı veya şifre hatalı.");

            CheckVipExpiry(user);
            AuthorizationService.SyncVipBadges(user);
            _userService.Update(user);

            CurrentUser = user;
            SessionService.Save(user.Id, rememberMe);
            return AuthResult.Ok(user, "Giriş başarılı.");
        }

        public AuthResult ChangePassword(Guid userId, string currentPassword, string newPassword)
        {
            var user = _userService.GetById(userId);
            if (user == null) return AuthResult.Fail("Kullanıcı bulunamadı.");
            if (!PasswordHasher.Verify(currentPassword, user.PasswordHash))
                return AuthResult.Fail("Mevcut şifre hatalı.");
            if (newPassword.Length < 6)
                return AuthResult.Fail("Yeni şifre en az 6 karakter olmalıdır.");

            user.PasswordHash = PasswordHasher.Hash(newPassword);
            _userService.Update(user);
            return AuthResult.Ok(user, "Şifre başarıyla değiştirildi.");
        }

        public User? TryRestoreSession()
        {
            var session = SessionService.Load();
            if (session == null) return null;

            var user = _userService.GetById(session.UserId);
            if (user == null)
            {
                SessionService.Clear();
                return null;
            }

            CheckVipExpiry(user);
            AuthorizationService.SyncVipBadges(user);
            CurrentUser = user;
            return user;
        }

        public void Logout()
        {
            CurrentUser = null;
            SessionService.Clear();
        }

        private void CheckVipExpiry(User user)
        {
            if (user.IsVip && user.VipEndDate.HasValue && user.VipEndDate.Value < DateTime.UtcNow)
            {
                _userService.RevokeVip(user.Id);
                user.IsVip = false;
                user.VipEndDate = null;
                user.MembershipLevel = "Ücretsiz";
            }
        }
    }
}
