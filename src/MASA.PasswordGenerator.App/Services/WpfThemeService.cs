using System.Windows;
using System.Windows.Media;
using MASA.PasswordGenerator.Core.Enums;
using MASA.PasswordGenerator.Core.Interfaces;

namespace MASA.PasswordGenerator.App.Services;

public class WpfThemeService
{
    private readonly ISettingsService _settingsService;

    public WpfThemeService(ISettingsService settingsService)
    {
        _settingsService = settingsService;
    }

    public void ApplyTheme(ThemeMode themeMode)
    {
        var app = System.Windows.Application.Current;
        if (app == null) return;

        bool isDark = themeMode switch
        {
            ThemeMode.Dark => true,
            ThemeMode.Light => false,
            ThemeMode.System => IsSystemDarkMode(),
            _ => true
        };

        if (isDark)
        {
            app.Resources["BgRootBrush"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#0F172A"));
            app.Resources["BgSidebarBrush"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1E293B"));
            app.Resources["BgCardBrush"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1E293B"));
            app.Resources["BgCardHoverBrush"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#334155"));
            app.Resources["BgInputBrush"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#0B1120"));
            app.Resources["BorderBrush"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#334155"));
            app.Resources["TextPrimaryBrush"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F8FAFC"));
            app.Resources["TextSecondaryBrush"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#94A3B8"));
            app.Resources["TextMutedBrush"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#64748B"));
        }
        else
        {
            app.Resources["BgRootBrush"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F1F5F9"));
            app.Resources["BgSidebarBrush"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFFFFF"));
            app.Resources["BgCardBrush"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFFFFF"));
            app.Resources["BgCardHoverBrush"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E2E8F0"));
            app.Resources["BgInputBrush"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F8FAFC"));
            app.Resources["BorderBrush"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#CBD5E1"));
            app.Resources["TextPrimaryBrush"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#0F172A"));
            app.Resources["TextSecondaryBrush"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#475569"));
            app.Resources["TextMutedBrush"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#94A3B8"));
        }
    }

    private static bool IsSystemDarkMode()
    {
        try
        {
            using var key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize");
            var value = key?.GetValue("AppsUseLightTheme");
            return value is int val && val == 0;
        }
        catch
        {
            return true; // Default fallback to dark
        }
    }
}
