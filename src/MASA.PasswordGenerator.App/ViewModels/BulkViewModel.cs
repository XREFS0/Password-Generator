using System.IO;
using System.Collections.ObjectModel;
using System.Text;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using MASA.PasswordGenerator.Core.Interfaces;
using MASA.PasswordGenerator.Core.Models;
using MASA.PasswordGenerator.Infrastructure.Export;

namespace MASA.PasswordGenerator.App.ViewModels;

public partial class BulkViewModel : ObservableObject
{
    private readonly IPasswordGenerator _passwordGenerator;
    private readonly IClipboardService _clipboardService;
    private readonly ISettingsService _settingsService;

    [ObservableProperty]
    private int _count = 10;

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
    private ObservableCollection<PasswordResult> _results = [];

    [ObservableProperty]
    private string _notificationMessage = string.Empty;

    [ObservableProperty]
    private bool _isNotificationVisible;

    public BulkViewModel(
        IPasswordGenerator passwordGenerator,
        IClipboardService clipboardService,
        ISettingsService settingsService)
    {
        _passwordGenerator = passwordGenerator;
        _clipboardService = clipboardService;
        _settingsService = settingsService;

        GenerateBulk();
    }

    [RelayCommand]
    public void GenerateBulk()
    {
        var options = new BulkOptions
        {
            Count = Count,
            PasswordOptions = new PasswordOptions
            {
                Length = Length,
                IncludeUppercase = IncludeUppercase,
                IncludeLowercase = IncludeLowercase,
                IncludeDigits = IncludeDigits,
                IncludeSymbols = IncludeSymbols,
                ExcludeSimilarCharacters = ExcludeSimilarCharacters,
                ExcludeAmbiguousSymbols = ExcludeAmbiguousSymbols
            }
        };

        var generated = _passwordGenerator.GenerateBulk(options);
        Results = new ObservableCollection<PasswordResult>(generated);
    }

    [RelayCommand]
    public async Task CopySingleAsync(PasswordResult? item)
    {
        if (item == null || string.IsNullOrEmpty(item.Value)) return;

        var settings = _settingsService.CurrentSettings;
        await _clipboardService.CopyToClipboardAsync(
            item.Value,
            settings.ClipboardAutoClearEnabled,
            settings.ClipboardAutoClearSeconds);

        ShowNotification("Copied selected password to clipboard!");
    }

    [RelayCommand]
    public async Task CopyAllAsync()
    {
        if (Results.Count == 0) return;

        var sb = new StringBuilder();
        foreach (var item in Results)
        {
            sb.AppendLine(item.Value);
        }

        var settings = _settingsService.CurrentSettings;
        await _clipboardService.CopyToClipboardAsync(
            sb.ToString().TrimEnd(),
            settings.ClipboardAutoClearEnabled,
            settings.ClipboardAutoClearSeconds);

        ShowNotification($"Copied all {Results.Count} passwords to clipboard!");
    }

    [RelayCommand]
    public async Task ExportAsync(string format)
    {
        if (Results.Count == 0) return;

        var saveDialog = new SaveFileDialog
        {
            FileName = $"MASA_Passwords_{DateTime.Now:yyyyMMdd_HHmmss}",
            Filter = format switch
            {
                "csv" => "CSV Files (*.csv)|*.csv",
                "json" => "JSON Files (*.json)|*.json",
                _ => "Text Files (*.txt)|*.txt"
            }
        };

        if (saveDialog.ShowDialog() == true)
        {
            try
            {
                if (format == "csv")
                {
                    await PasswordExportService.ExportToCsvAsync(saveDialog.FileName, Results);
                }
                else if (format == "json")
                {
                    await PasswordExportService.ExportToJsonAsync(saveDialog.FileName, Results);
                }
                else
                {
                    await PasswordExportService.ExportToTxtAsync(saveDialog.FileName, Results);
                }

                ShowNotification($"Successfully exported to {Path.GetFileName(saveDialog.FileName)}!");
            }
            catch (Exception ex)
            {
                ShowNotification($"Export failed: {ex.Message}");
            }
        }
    }

    private void ShowNotification(string msg)
    {
        NotificationMessage = msg;
        IsNotificationVisible = true;
        _ = Task.Run(async () =>
        {
            await Task.Delay(3000);
            IsNotificationVisible = false;
        });
    }
}
