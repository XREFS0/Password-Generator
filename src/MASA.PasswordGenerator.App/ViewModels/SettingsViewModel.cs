using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MASA.PasswordGenerator.App.Services;
using MASA.PasswordGenerator.Core.Enums;
using MASA.PasswordGenerator.Core.Interfaces;

namespace MASA.PasswordGenerator.App.ViewModels;

public partial class SettingsViewModel : ObservableObject
{
    private readonly ISettingsService _settingsService;
    private readonly WpfThemeService _themeService;

    [ObservableProperty]
    private ThemeMode _selectedTheme;

    [ObservableProperty]
    private int _defaultLength;

    [ObservableProperty]
    private bool _defaultUppercase;

    [ObservableProperty]
    private bool _defaultLowercase;

    [ObservableProperty]
    private bool _defaultDigits;

    [ObservableProperty]
    private bool _defaultSymbols;

    [ObservableProperty]
    private bool _defaultExcludeSimilar;

    [ObservableProperty]
    private bool _defaultExcludeAmbiguous;

    [ObservableProperty]
    private bool _clipboardAutoClear;

    [ObservableProperty]
    private int _clipboardAutoClearSeconds;

    [ObservableProperty]
    private bool _historyEnabled;

    [ObservableProperty]
    private bool _breachCheckEnabled;

    [ObservableProperty]
    private string _notification = string.Empty;

    [ObservableProperty]
    private bool _isNotificationVisible;

    public SettingsViewModel(ISettingsService settingsService, WpfThemeService themeService)
    {
        _settingsService = settingsService;
        _themeService = themeService;

        var s = _settingsService.CurrentSettings;
        SelectedTheme = s.Theme;
        DefaultLength = s.DefaultLength;
        DefaultUppercase = s.DefaultUppercase;
        DefaultLowercase = s.DefaultLowercase;
        DefaultDigits = s.DefaultDigits;
        DefaultSymbols = s.DefaultSymbols;
        DefaultExcludeSimilar = s.DefaultExcludeSimilar;
        DefaultExcludeAmbiguous = s.DefaultExcludeAmbiguous;
        ClipboardAutoClear = s.ClipboardAutoClearEnabled;
        ClipboardAutoClearSeconds = s.ClipboardAutoClearSeconds;
        HistoryEnabled = s.HistoryEnabled;
        BreachCheckEnabled = s.BreachCheckEnabled;
    }

    partial void OnSelectedThemeChanged(ThemeMode value)
    {
        _themeService.ApplyTheme(value);
        SaveSettings();
    }

    [RelayCommand]
    public void SaveSettings()
    {
        var s = _settingsService.CurrentSettings;
        s.Theme = SelectedTheme;
        s.DefaultLength = DefaultLength;
        s.DefaultUppercase = DefaultUppercase;
        s.DefaultLowercase = DefaultLowercase;
        s.DefaultDigits = DefaultDigits;
        s.DefaultSymbols = DefaultSymbols;
        s.DefaultExcludeSimilar = DefaultExcludeSimilar;
        s.DefaultExcludeAmbiguous = DefaultExcludeAmbiguous;
        s.ClipboardAutoClearEnabled = ClipboardAutoClear;
        s.ClipboardAutoClearSeconds = ClipboardAutoClearSeconds;
        s.HistoryEnabled = HistoryEnabled;
        s.BreachCheckEnabled = BreachCheckEnabled;

        _ = _settingsService.SaveSettingsAsync();

        Notification = "Settings saved successfully!";
        IsNotificationVisible = true;
        _ = Task.Run(async () =>
        {
            await Task.Delay(2500);
            IsNotificationVisible = false;
        });
    }
}
