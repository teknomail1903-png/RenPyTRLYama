using System;
using System.IO;
using System.Windows;
using RenPyTRLauncher.Services;
using RenPyTRLauncher.Views;

namespace RenPyTRLauncher
{
    public partial class App : System.Windows.Application
    {
        private static readonly string LogPath = Path.Combine(AppContext.BaseDirectory, "logs", "startup.log");

        public static void Log(string message)
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(LogPath)!);
                File.AppendAllText(LogPath, $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] {message}{Environment.NewLine}");
            }
            catch
            {
                // Ignore logging errors
            }
        }

        private void App_OnStartup(object sender, StartupEventArgs e)
        {
            try
            {
                Log("App Started");

                Log("Initializing Services...");
                AppBootstrap.InitializeServices();
                Log("Services Loaded");

                Log("Database Initialized");

                Log("Checking for restored session...");
                var restored = ServiceLocator.AuthService?.TryRestoreSession();
                if (restored != null)
                {
                    Log("Session restored, opening MainWindow");
                    Log("MainWindow Created");
                    new MainWindow().Show();
                    Log("MainWindow Shown");
                }
                else
                {
                    Log("No session found, opening LoginWindow");
                    Log("LoginWindow Created");
                    new LoginWindow().Show();
                    Log("LoginWindow Shown");
                }
            }
            catch (Exception ex)
            {
                Log($"ERROR: {ex.GetType().Name} - {ex.Message}");
                Log($"STACK TRACE: {ex.StackTrace}");
                MessageBox.Show($"Uygulama başlatılırken hata oluştu:\n\n{ex.Message}\n\nDetaylar için logs/startup.log dosyasını kontrol edin.", 
                    "Başlatma Hatası", MessageBoxButton.OK, MessageBoxImage.Error);
                Shutdown();
            }
        }
    }
}
