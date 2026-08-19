using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MASA.PasswordGenerator.Core.Enums;
using MASA.PasswordGenerator.Core.Interfaces;
using MASA.PasswordGenerator.Core.Models;

namespace MASA.PasswordGenerator.App.ViewModels;

public partial class GeneratorViewModel : ObservableObject
{
    private readonly IPasswordGenerator _passwordGenerator;
    private readonly IClipboardService _clipboardService;
    private readonly ISettingsService _settingsService;
    private readonly IHistoryRepository _historyRepository;

    [ObservableProperty]
    private int _length = 16;

    [ObservableProperty]
    private bool _includeUppercase = true;

    [ObservableProperty]
    private bool _includeLowercase = true;

    [ObservableProperty]
    private bool _includeDigits = true;

    [ObservableProperty]
    private bool _includeSymbols = true;

    [ObservableProperty]
    private bool _excludeSimilarCharacters = false;

    [ObservableProperty]
    private bool _excludeAmbiguousSymbols = false;

    [ObservableProperty]
    private string _customCharacters = string.Empty;

    [ObservableProperty]
    private bool _useCustomCharactersOnly = false;

    [ObservableProperty]
    private string _generatedPassword = string.Empty;

    [ObservableProperty]
    private StrengthResult? _strength;

    [ObservableProperty]
    private string _copyNotification = string.Empty;

    [ObservableProperty]
    private bool _isNotificationVisible;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    public GeneratorViewModel(
        IPasswordGenerator passwordGenerator,
        IClipboardService clipboardService,
        ISettingsService settingsService,
        IHistoryRepository historyRepository)
    {
        _passwordGenerator = passwordGenerator;
        _clipboardService = clipboardService;
        _settingsService = settingsService;
        _historyRepository = historyRepository;

        // Apply defaults from settings
        var settings = _settingsService.CurrentSettings;
        Length = settings.DefaultLength;
        IncludeUppercase = settings.DefaultUppercase;
        IncludeLowercase = settings.DefaultLowercase;
        IncludeDigits = settings.DefaultDigits;
        IncludeSymbols = settings.DefaultSymbols;
        ExcludeSimilarCharacters = settings.DefaultExcludeSimilar;
        ExcludeAmbiguousSymbols = settings.DefaultExcludeAmbiguous;

        GeneratePassword();
    }

    [RelayCommand]
    public void GeneratePassword()
    {
        ErrorMessage = string.Empty;
        try
        {
            var options = new PasswordOptions
            {
                Length = Length,
                IncludeUppercase = IncludeUppercase,
                IncludeLowercase = IncludeLowercase,
                IncludeDigits = IncludeDigits,
                IncludeSymbols = IncludeSymbols,
                ExcludeSimilarCharacters = ExcludeSimilarCharacters,
                ExcludeAmbiguousSymbols = ExcludeAmbiguousSymbols,
                CustomCharacters = CustomCharacters,
                UseCustomCharactersOnly = UseCustomCharactersOnly
            };

            var result = _passwordGenerator.Generate(options);
            GeneratedPassword = result.Value;
            Strength = result.Strength;

            if (_settingsService.CurrentSettings.HistoryEnabled)
            {
                _ = _historyRepository.AddAsync(new HistoryEntry
                {
                    Password = result.Value,
                    Length = result.Length,
                    Strength = result.Strength.Strength,
                    EntropyBits = result.Strength.EntropyBits,
                    GeneratorType = "Generator"
                });
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }
    }

    [RelayCommand]
    public async Task CopyPasswordAsync()
    {
        if (string.IsNullOrEmpty(GeneratedPassword)) return;

        var settings = _settingsService.CurrentSettings;
        await _clipboardService.CopyToClipboardAsync(
            GeneratedPassword,
            settings.ClipboardAutoClearEnabled,
            settings.ClipboardAutoClearSeconds);

        CopyNotification = settings.ClipboardAutoClearEnabled
            ? $"Copied! (Auto-clearing in {settings.ClipboardAutoClearSeconds}s)"
            : "Copied to clipboard!";

        IsNotificationVisible = true;
        _ = Task.Run(async () =>
        {
            await Task.Delay(3000);
            IsNotificationVisible = false;
        });
    }

    [RelayCommand]
    public void ApplyPreset(string presetName)
    {
        switch (presetName)
        {
            case "Simple":
                Length = 12;
                IncludeUppercase = true;
                IncludeLowercase = true;
                IncludeDigits = true;
                IncludeSymbols = false;
                UseCustomCharactersOnly = false;
                break;
            case "Strong":
                Length = 16;
                IncludeUppercase = true;
                IncludeLowercase = true;
                IncludeDigits = true;
                IncludeSymbols = true;
                UseCustomCharactersOnly = false;
                break;
            case "Max":
                Length = 32;
                IncludeUppercase = true;
                IncludeLowercase = true;
                IncludeDigits = true;
                IncludeSymbols = true;
                ExcludeSimilarCharacters = false;
                ExcludeAmbiguousSymbols = false;
                UseCustomCharactersOnly = false;
                break;
        }

        GeneratePassword();
    }
}
