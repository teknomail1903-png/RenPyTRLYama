namespace RenPyTRLauncher.Models
{
    public class AuthResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public User? User { get; set; }

        public static AuthResult Ok(User user, string message = "") =>
            new() { Success = true, User = user, Message = message };

        public static AuthResult Fail(string message) =>
            new() { Success = false, Message = message };
    }
}
