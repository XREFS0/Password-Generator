using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MASA.PasswordGenerator.Core.Enums;
using MASA.PasswordGenerator.Core.Interfaces;
using MASA.PasswordGenerator.Core.Models;

namespace MASA.PasswordGenerator.App.ViewModels;

public partial class DashboardViewModel : ObservableObject
{
    private readonly IPasswordGenerator _passwordGenerator;
    private readonly IPassphraseGenerator _passphraseGenerator;
    private readonly IPinGenerator _pinGenerator;
    private readonly IClipboardService _clipboardService;
    private readonly ISettingsService _settingsService;
    private readonly IHistoryRepository _historyRepository;
    private readonly Action<string> _navigateAction;

    [ObservableProperty]
    private string _quickPassword = string.Empty;

    [ObservableProperty]
    private StrengthResult? _quickStrength;

    [ObservableProperty]
    private string _copyStatusMessage = string.Empty;

    [ObservableProperty]
    private bool _isCopyStatusVisible;

    public DashboardViewModel(
        IPasswordGenerator passwordGenerator,
        IPassphraseGenerator passphraseGenerator,
        IPinGenerator pinGenerator,
        IClipboardService clipboardService,
        ISettingsService settingsService,
        IHistoryRepository historyRepository,
        Action<string> navigateAction)
    {
        _passwordGenerator = passwordGenerator;
        _passphraseGenerator = passphraseGenerator;
        _pinGenerator = pinGenerator;
        _clipboardService = clipboardService;
        _settingsService = settingsService;
        _historyRepository = historyRepository;
        _navigateAction = navigateAction;

        GenerateStrongPassword();
    }

    [RelayCommand]
    public void GenerateStrongPassword()
    {
        var result = _passwordGenerator.Generate(PasswordOptions.Strong);
        QuickPassword = result.Value;
        QuickStrength = result.Strength;
        SaveToHistoryIfAllowed(result, "Dashboard Strong");
    }

    [RelayCommand]
    public void GeneratePassphrase()
    {
        var result = _passphraseGenerator.Generate(new PassphraseOptions { WordCount = 4, Separator = "-" });
        QuickPassword = result.Value;
        QuickStrength = result.Strength;
        SaveToHistoryIfAllowed(result, "Dashboard Passphrase");
    }

    [RelayCommand]
    public void GeneratePin()
    {
        var result = _pinGenerator.Generate(new PinOptions { Length = 6 });
        QuickPassword = result.Value;
        QuickStrength = result.Strength;
        SaveToHistoryIfAllowed(result, "Dashboard PIN");
    }

    [RelayCommand]
    public async Task CopyPasswordAsync()
    {
        if (string.IsNullOrEmpty(QuickPassword)) return;

        var settings = _settingsService.CurrentSettings;
        await _clipboardService.CopyToClipboardAsync(
            QuickPassword,
            settings.ClipboardAutoClearEnabled,
            settings.ClipboardAutoClearSeconds);

        CopyStatusMessage = settings.ClipboardAutoClearEnabled
            ? $"Copied! (Auto-clears in {settings.ClipboardAutoClearSeconds}s)"
            : "Copied to clipboard!";

        IsCopyStatusVisible = true;
        _ = Task.Run(async () =>
        {
            await Task.Delay(3000);
            IsCopyStatusVisible = false;
        });
    }

    [RelayCommand]
    public void Navigate(string target)
    {
        _navigateAction(target);
    }

    private void SaveToHistoryIfAllowed(PasswordResult result, string type)
    {
        if (_settingsService.CurrentSettings.HistoryEnabled)
        {
            _ = _historyRepository.AddAsync(new HistoryEntry
            {
                Password = result.Value,
                Length = result.Length,
                Strength = result.Strength.Strength,
                EntropyBits = result.Strength.EntropyBits,
                GeneratorType = type
            });
        }
    }
}
