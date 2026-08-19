using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MASA.PasswordGenerator.Core.Interfaces;
using MASA.PasswordGenerator.Core.Models;

namespace MASA.PasswordGenerator.App.ViewModels;

public partial class PresetsViewModel : ObservableObject
{
    private readonly IPolicyEvaluator _policyEvaluator;
    private readonly IPasswordGenerator _passwordGenerator;
    private readonly IClipboardService _clipboardService;
    private readonly ISettingsService _settingsService;

    [ObservableProperty]
    private ObservableCollection<PasswordPolicy> _policies = [];

    [ObservableProperty]
    private PasswordPolicy? _selectedPolicy;

    [ObservableProperty]
    private string _testPassword = string.Empty;

    [ObservableProperty]
    private PolicyValidationResult? _validationResult;

    [ObservableProperty]
    private string _generatedPolicyPassword = string.Empty;

    [ObservableProperty]
    private string _customCharSetName = string.Empty;

    [ObservableProperty]
    private string _customCharSetCharacters = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";

    [ObservableProperty]
    private ObservableCollection<CustomPreset> _customPresets = [];

    [ObservableProperty]
    private string _notification = string.Empty;

    [ObservableProperty]
    private bool _isNotificationVisible;

    public PresetsViewModel(
        IPolicyEvaluator policyEvaluator,
        IPasswordGenerator passwordGenerator,
        IClipboardService clipboardService,
        ISettingsService settingsService)
    {
        _policyEvaluator = policyEvaluator;
        _passwordGenerator = passwordGenerator;
        _clipboardService = clipboardService;
        _settingsService = settingsService;

        var builtin = _policyEvaluator.GetBuiltinPolicies();
        Policies = new ObservableCollection<PasswordPolicy>(builtin);
        SelectedPolicy = Policies.FirstOrDefault();

        CustomPresets =
        [
            new CustomPreset
            {
                Name = "Base58 Crypto Set",
                Description = "Bitcoin/Crypto friendly (No 0, O, I, l)",
                Options = new PasswordOptions
                {
                    Length = 20,
                    CustomCharacters = "123456789ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz",
                    UseCustomCharactersOnly = true
                }
            },
            new CustomPreset
            {
                Name = "Hexadecimal Key",
                Description = "Pure 0-9 & A-F cryptographic token",
                Options = new PasswordOptions
                {
                    Length = 32,
                    CustomCharacters = "0123456789ABCDEF",
                    UseCustomCharactersOnly = true
                }
            }
        ];

        ValidateTestPassword();
    }

    partial void OnSelectedPolicyChanged(PasswordPolicy? value)
    {
        ValidateTestPassword();
    }

    partial void OnTestPasswordChanged(string value)
    {
        ValidateTestPassword();
    }

    [RelayCommand]
    public void ValidateTestPassword()
    {
        if (SelectedPolicy != null)
        {
            ValidationResult = _policyEvaluator.Validate(TestPassword, SelectedPolicy);
        }
    }

    [RelayCommand]
    public void GenerateCompliantPassword()
    {
        if (SelectedPolicy == null) return;

        var options = new PasswordOptions
        {
            Length = Math.Max(SelectedPolicy.MinLength, 16),
            IncludeUppercase = SelectedPolicy.RequireUppercase,
            IncludeLowercase = SelectedPolicy.RequireLowercase,
            IncludeDigits = SelectedPolicy.RequireDigit,
            IncludeSymbols = SelectedPolicy.RequireSymbol
        };

        var result = _passwordGenerator.Generate(options);
        GeneratedPolicyPassword = result.Value;
        TestPassword = result.Value;
        ValidateTestPassword();
    }

    [RelayCommand]
    public async Task CopyGeneratedAsync()
    {
        if (string.IsNullOrEmpty(GeneratedPolicyPassword)) return;
        var settings = _settingsService.CurrentSettings;
        await _clipboardService.CopyToClipboardAsync(
            GeneratedPolicyPassword,
            settings.ClipboardAutoClearEnabled,
            settings.ClipboardAutoClearSeconds);

        ShowNotification("Compliant password copied to clipboard!");
    }

    [RelayCommand]
    public void AddCustomPreset()
    {
        if (string.IsNullOrWhiteSpace(CustomCharSetName) || string.IsNullOrWhiteSpace(CustomCharSetCharacters))
        {
            ShowNotification("Please provide both a Name and Characters for the preset.");
            return;
        }

        CustomPresets.Add(new CustomPreset
        {
            Name = CustomCharSetName.Trim(),
            Description = $"Custom ({CustomCharSetCharacters.Length} chars)",
            Options = new PasswordOptions
            {
                Length = 16,
                CustomCharacters = CustomCharSetCharacters.Trim(),
                UseCustomCharactersOnly = true
            }
        });

        CustomCharSetName = string.Empty;
        ShowNotification("New custom preset created!");
    }

    [RelayCommand]
    public void DeleteCustomPreset(CustomPreset? preset)
    {
        if (preset == null) return;
        CustomPresets.Remove(preset);
        ShowNotification("Preset removed.");
    }

    private void ShowNotification(string msg)
    {
        Notification = msg;
        IsNotificationVisible = true;
        _ = Task.Run(async () =>
        {
            await Task.Delay(3000);
            IsNotificationVisible = false;
        });
    }
}
