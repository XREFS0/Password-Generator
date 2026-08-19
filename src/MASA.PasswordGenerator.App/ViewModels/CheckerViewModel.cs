using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MASA.PasswordGenerator.Core.Interfaces;
using MASA.PasswordGenerator.Core.Models;

namespace MASA.PasswordGenerator.App.ViewModels;

public partial class CheckerViewModel : ObservableObject
{
    private readonly IStrengthAnalyzer _strengthAnalyzer;
    private readonly IPasswordBreachChecker _breachChecker;
    private readonly ISettingsService _settingsService;

    [ObservableProperty]
    private string _passwordInput = string.Empty;

    [ObservableProperty]
    private bool _isPasswordVisible = false;

    [ObservableProperty]
    private StrengthResult? _strengthResult;

    [ObservableProperty]
    private bool _isBreachCheckRunning = false;

    [ObservableProperty]
    private BreachCheckResult? _breachResult;

    [ObservableProperty]
    private bool _isBreachEnabled;

    public CheckerViewModel(
        IStrengthAnalyzer strengthAnalyzer,
        IPasswordBreachChecker breachChecker,
        ISettingsService settingsService)
    {
        _strengthAnalyzer = strengthAnalyzer;
        _breachChecker = breachChecker;
        _settingsService = settingsService;
        IsBreachEnabled = _settingsService.CurrentSettings.BreachCheckEnabled;

        Analyze();
    }

    partial void OnPasswordInputChanged(string value)
    {
        Analyze();
    }

    [RelayCommand]
    public void TogglePasswordVisibility()
    {
        IsPasswordVisible = !IsPasswordVisible;
    }

    [RelayCommand]
    public void Analyze()
    {
        StrengthResult = _strengthAnalyzer.Analyze(PasswordInput);
    }

    [RelayCommand]
    public async Task CheckBreachAsync()
    {
        if (string.IsNullOrEmpty(PasswordInput)) return;

        IsBreachCheckRunning = true;
        try
        {
            BreachResult = await _breachChecker.CheckBreachAsync(PasswordInput);
        }
        finally
        {
            IsBreachCheckRunning = false;
        }
    }
}
