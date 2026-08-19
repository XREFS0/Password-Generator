using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MASA.PasswordGenerator.Core.Enums;
using MASA.PasswordGenerator.Core.Interfaces;
using MASA.PasswordGenerator.Core.Models;

namespace MASA.PasswordGenerator.App.ViewModels;

public partial class PassphraseViewModel : ObservableObject
{
    private readonly IPassphraseGenerator _passphraseGenerator;
    private readonly IClipboardService _clipboardService;
    private readonly ISettingsService _settingsService;
    private readonly IHistoryRepository _historyRepository;

    [ObservableProperty]
    private int _wordCount = 4;

    [ObservableProperty]
    private string _separator = "-";

    [ObservableProperty]
    private PassphraseCasing _casing = PassphraseCasing.Lowercase;

    [ObservableProperty]
    private bool _includeNumber = true;

    [ObservableProperty]
    private bool _capitalizeFirstLetter = false;

    [ObservableProperty]
    private string _generatedPassphrase = string.Empty;

    [ObservableProperty]
    private StrengthResult? _strength;

    [ObservableProperty]
    private string _copyNotification = string.Empty;

    [ObservableProperty]
    private bool _isNotificationVisible;

    public PassphraseViewModel(
        IPassphraseGenerator passphraseGenerator,
        IClipboardService clipboardService,
        ISettingsService settingsService,
        IHistoryRepository historyRepository)
    {
        _passphraseGenerator = passphraseGenerator;
        _clipboardService = clipboardService;
        _settingsService = settingsService;
        _historyRepository = historyRepository;

        Generate();
    }

    [RelayCommand]
    public void Generate()
    {
        var options = new PassphraseOptions
        {
            WordCount = WordCount,
            Separator = Separator,
            Casing = Casing,
            IncludeNumber = IncludeNumber,
            CapitalizeFirstLetter = CapitalizeFirstLetter
        };

        var result = _passphraseGenerator.Generate(options);
        GeneratedPassphrase = result.Value;
        Strength = result.Strength;

        if (_settingsService.CurrentSettings.HistoryEnabled)
        {
            _ = _historyRepository.AddAsync(new HistoryEntry
            {
                Password = result.Value,
                Length = result.Length,
                Strength = result.Strength.Strength,
                EntropyBits = result.Strength.EntropyBits,
                GeneratorType = "Passphrase"
            });
        }
    }

    [RelayCommand]
    public async Task CopyAsync()
    {
        if (string.IsNullOrEmpty(GeneratedPassphrase)) return;

        var settings = _settingsService.CurrentSettings;
        await _clipboardService.CopyToClipboardAsync(
            GeneratedPassphrase,
            settings.ClipboardAutoClearEnabled,
            settings.ClipboardAutoClearSeconds);

        CopyNotification = "Copied passphrase to clipboard!";
        IsNotificationVisible = true;
        _ = Task.Run(async () =>
        {
            await Task.Delay(3000);
            IsNotificationVisible = false;
        });
    }
}
