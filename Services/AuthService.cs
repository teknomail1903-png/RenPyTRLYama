using System.Linq;
using RenPyTRLauncher.Models;

namespace RenPyTRLauncher.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserService _userService;
        private static readonly string LogPath = System.IO.Path.Combine(
            System.AppContext.BaseDirectory, "logs", "auth.log");

        public AuthService(IUserService userService)
        {
            _userService = userService;
        }

        private static void Log(string message)
        {
            try
            {
                var dir = System.IO.Path.GetDirectoryName(LogPath);
                if (!string.IsNullOrEmpty(dir))
                    System.IO.Directory.CreateDirectory(dir);
                System.IO.File.AppendAllText(LogPath,
                    $"[{System.DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] {message}{System.Environment.NewLine}");
            }
            catch
            {
                // Ignore logging errors
            }
        }

        public User? CurrentUser { get; private set; }

        public AuthResult Register(string username, string email, string password, string secretQuestion = "", string secretAnswer = "")
        {
            username = username?.Trim() ?? "";
            email = email?.Trim() ?? "";
            secretQuestion = secretQuestion?.Trim() ?? "";
            secretAnswer = secretAnswer?.Trim() ?? "";

            Log($"Register attempt - Username: {username}, Email: {email}");

            if (username.Length < 3)
            {
                Log($"Register failed - Username too short: {username}");
                return AuthResult.Fail("Kullanıcı adı en az 3 karakter olmalıdır.");
            }
            if (password.Length < 6)
            {
                Log($"Register failed - Password too short for: {username}");
                return AuthResult.Fail("Şifre en az 6 karakter olmalıdır.");
            }
            if (!email.Contains('@'))
            {
                Log($"Register failed - Invalid email format: {email}");
                return AuthResult.Fail("Geçerli bir e-posta adresi girin.");
            }

            if (_userService.GetByUsername(username) != null)
            {
                Log($"Register failed - Username already exists: {username}");
                return AuthResult.Fail("Bu kullanıcı adı zaten kullanılıyor.");
            }
            if (_userService.GetByEmail(email) != null)
            {
                Log($"Register failed - Email already exists: {email}");
                return AuthResult.Fail("Bu e-posta zaten kayıtlı.");
            }

            var user = new User
            {
                Username = username,
                Email = email,
                PasswordHash = PasswordHasher.Hash(password),
                Role = UserRole.User,
                MembershipLevel = "Ücretsiz",
                AvatarPath = ImageService.GetDefaultAvatar(username),
                SecretQuestion = secretQuestion,
                SecretAnswer = secretAnswer
            };
            AuthorizationService.SyncVipBadges(user);
            _userService.Create(user);

            CurrentUser = user;
            Log($"Register successful - User ID: {user.Id}, Username: {username}");
            return AuthResult.Ok(user, "Kayıt başarılı. Hoş geldiniz!");
        }

        public AuthResult Login(string usernameOrEmail, string password, bool rememberMe)
        {
            usernameOrEmail = usernameOrEmail?.Trim() ?? "";
            Log($"Login attempt - Username/Email: {usernameOrEmail}, RememberMe: {rememberMe}");

            if (string.IsNullOrEmpty(usernameOrEmail) || string.IsNullOrEmpty(password))
            {
                Log($"Login failed - Empty credentials");
                return AuthResult.Fail("Kullanıcı adı ve şifre gereklidir.");
            }

            var user = _userService.GetByUsername(usernameOrEmail)
                       ?? _userService.GetByEmail(usernameOrEmail);

            if (user == null || !PasswordHasher.Verify(password, user.PasswordHash))
            {
                Log($"Login failed - Invalid credentials for: {usernameOrEmail}");
                return AuthResult.Fail("Kullanıcı adı veya şifre hatalı.");
            }

            CheckVipExpiry(user);
            AuthorizationService.SyncVipBadges(user);
            _userService.Update(user);

            CurrentUser = user;
            SessionService.Save(user.Id, rememberMe);
            Log($"Login successful - User ID: {user.Id}, Username: {user.Username}");
            return AuthResult.Ok(user, "Giriş başarılı.");
        }

        public AuthResult ChangePassword(Guid userId, string currentPassword, string newPassword)
        {
            Log($"ChangePassword attempt - User ID: {userId}");
            var user = _userService.GetById(userId);
            if (user == null)
            {
                Log($"ChangePassword failed - User not found: {userId}");
                return AuthResult.Fail("Kullanıcı bulunamadı.");
            }
            if (!PasswordHasher.Verify(currentPassword, user.PasswordHash))
            {
                Log($"ChangePassword failed - Invalid current password for: {user.Username}");
                return AuthResult.Fail("Mevcut şifre hatalı.");
            }
            if (newPassword.Length < 6)
            {
                Log($"ChangePassword failed - New password too short for: {user.Username}");
                return AuthResult.Fail("Yeni şifre en az 6 karakter olmalıdır.");
            }

            user.PasswordHash = PasswordHasher.Hash(newPassword);
            _userService.Update(user);
            Log($"ChangePassword successful - User ID: {userId}");
            return AuthResult.Ok(user, "Şifre başarıyla değiştirildi.");
        }

        public AuthResult ResetPassword(Guid userId, string newPassword)
        {
            Log($"ResetPassword attempt - User ID: {userId}");
            var user = _userService.GetById(userId);
            if (user == null)
            {
                Log($"ResetPassword failed - User not found: {userId}");
                return AuthResult.Fail("Kullanıcı bulunamadı.");
            }
            if (newPassword.Length < 6)
            {
                Log($"ResetPassword failed - New password too short for: {user.Username}");
                return AuthResult.Fail("Yeni şifre en az 6 karakter olmalıdır.");
            }

            user.PasswordHash = PasswordHasher.Hash(newPassword);
            _userService.Update(user);
            Log($"ResetPassword successful - User ID: {userId}");
            return AuthResult.Ok(user, "Şifre başarıyla sıfırlandı.");
        }

        public User? TryRestoreSession()
        {
            Log($"TryRestoreSession attempt");
            var session = SessionService.Load();
            if (session == null)
            {
                Log($"TryRestoreSession failed - No session found");
                return null;
            }

            var user = _userService.GetById(session.UserId);
            if (user == null)
            {
                Log($"TryRestoreSession failed - User not found: {session.UserId}");
                SessionService.Clear();
                return null;
            }

            CheckVipExpiry(user);
            AuthorizationService.SyncVipBadges(user);
            CurrentUser = user;
            Log($"TryRestoreSession successful - User ID: {user.Id}, Username: {user.Username}");
            return user;
        }

        public void Logout()
        {
            var userId = CurrentUser?.Id;
            var username = CurrentUser?.Username;
            CurrentUser = null;
            SessionService.Clear();
            Log($"Logout - User ID: {userId}, Username: {username}");
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
