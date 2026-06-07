using System.Windows;
using System.Windows.Media;
using RenPyTRLauncher.Models;

namespace RenPyTRLauncher.Services
{
    public static class ThemeService
    {
        public const string SteamDark = "SteamDark";
        public const string DiscordDark = "DiscordDark";

        public static string CurrentTheme { get; private set; } = SteamDark;

        public static void LoadFromSettings(ISettingsService? settings)
        {
            if (settings == null) return;
            CurrentTheme = settings.Get(AppSettingKeys.Theme, SteamDark);
        }

        public static void SaveTheme(ISettingsService settings, string theme)
        {
            CurrentTheme = theme;
            settings.Set(AppSettingKeys.Theme, theme);
        }

        public static void Apply(Window window)
        {
            var palette = GetPalette(CurrentTheme);
            window.Background = palette.WindowBg;
            ApplyToElement(window, palette);
        }

        private static void ApplyToElement(DependencyObject element, ThemePalette palette)
        {
            if (element is FrameworkElement fe && fe.Resources != null)
            {
                fe.Resources["AccentColor"] = palette.Accent;
                fe.Resources["AccentBrush"] = palette.AccentBrush;
                fe.Resources["SurfaceBrush"] = palette.SurfaceBrush;
                fe.Resources["CardBrush"] = palette.CardBrush;
            }
        }

        public static ThemePalette GetPalette(string theme) => theme switch
        {
            DiscordDark => new ThemePalette
            {
                Name = "Discord Dark",
                WindowBg = new SolidColorBrush(Color.FromRgb(0x31, 0x32, 0x36)),
                SurfaceBrush = new SolidColorBrush(Color.FromRgb(0x2B, 0x2D, 0x31)),
                CardBrush = new SolidColorBrush(Color.FromRgb(0x36, 0x38, 0x3F)),
                Accent = Color.FromRgb(0x58, 0x65, 0xF2),
                SidebarBg = Color.FromRgb(0x2B, 0x2D, 0x31),
                SidebarBorder = Color.FromRgb(0x1E, 0x1F, 0x22),
                SidebarActiveBg = Color.FromRgb(0x3C, 0x3F, 0x45),
                TextMuted = Color.FromRgb(0xB9, 0xBB, 0xBE)
            },
            _ => new ThemePalette
            {
                Name = "Steam Dark",
                WindowBg = new SolidColorBrush(Color.FromRgb(0x0D, 0x0D, 0x0F)),
                SurfaceBrush = new SolidColorBrush(Color.FromRgb(0x1A, 0x1A, 0x1F)),
                CardBrush = new SolidColorBrush(Color.FromRgb(0x22, 0x22, 0x28)),
                Accent = Color.FromRgb(0x66, 0xC0, 0xF4),
                SidebarBg = Color.FromRgb(0x17, 0x1A, 0x21),
                SidebarBorder = Color.FromRgb(0x2A, 0x3F, 0x5F),
                SidebarActiveBg = Color.FromRgb(0x1B, 0x28, 0x38),
                TextMuted = Color.FromRgb(0x8F, 0x98, 0xA0)
            }
        };

        public static string[] AvailableThemes => new[] { SteamDark, DiscordDark };

        public static string GetDisplayName(string theme) => theme switch
        {
            DiscordDark => "Discord Dark Theme",
            _ => "Steam Dark Theme"
        };
    }

    public class ThemePalette
    {
        public string Name { get; set; } = "";
        public Brush WindowBg { get; set; } = Brushes.Black;
        public Brush SurfaceBrush { get; set; } = Brushes.Gray;
        public Brush CardBrush { get; set; } = Brushes.DarkGray;
        public Color Accent { get; set; }
        public SolidColorBrush AccentBrush => new(Accent);
        public Color SidebarBg { get; set; }
        public Color SidebarBorder { get; set; }
        public Color SidebarActiveBg { get; set; }
        public Color TextMuted { get; set; }
    }
}
