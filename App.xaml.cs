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

        static App()
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(LogPath)!);
                File.AppendAllText(LogPath, $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] [STATIC CONSTRUCTOR] Static App constructor called{Environment.NewLine}");
            }
            catch
            {
                // Ignore logging errors
            }
        }

        public App()
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(LogPath)!);
                File.AppendAllText(LogPath, $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] [APP CONSTRUCTOR] App constructor called{Environment.NewLine}");
                
                // Add exception handling
                DispatcherUnhandledException += App_DispatcherUnhandledException;
                AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            }
            catch
            {
                // Ignore logging errors
            }
        }

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

        private void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            Log($"[DISPATCHER EXCEPTION] {e.Exception.Message}\n{e.Exception.StackTrace}");
            MessageBox.Show($"Dispatcher Exception: {e.Exception.Message}\n\n{e.Exception.StackTrace}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            e.Handled = true;
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (e.ExceptionObject is Exception ex)
            {
                Log($"[UNHANDLED EXCEPTION] {ex.Message}\n{ex.StackTrace}");
                MessageBox.Show($"Unhandled Exception: {ex.Message}\n\n{ex.StackTrace}", "Critical Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void App_OnStartup(object sender, StartupEventArgs e)
        {
            Log("[STARTUP] App_OnStartup called");
            
            try
            {
                Log("[STARTUP] Calling AppBootstrap.InitializeServices");
                Services.AppBootstrap.InitializeServices();
                Log("[STARTUP] AppBootstrap.InitializeServices completed");
            }
            catch (Exception ex)
            {
                Log($"[STARTUP] AppBootstrap.InitializeServices failed: {ex.Message}");
                Log($"[STARTUP] StackTrace: {ex.StackTrace}");
                MessageBox.Show($"Database initialization failed: {ex.Message}", "Startup Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            
            var modernLauncherWindow = new ModernLauncherWindow();
            modernLauncherWindow.Show();
        }
    }
}
