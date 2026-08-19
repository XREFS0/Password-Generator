using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MASA.PasswordGenerator.Core.Interfaces;
using MASA.PasswordGenerator.Core.Models;

namespace MASA.PasswordGenerator.App.ViewModels;

public partial class HistoryViewModel : ObservableObject
{
    private readonly IHistoryRepository _historyRepository;
    private readonly IClipboardService _clipboardService;
    private readonly ISettingsService _settingsService;

    [ObservableProperty]
    private bool _isHistoryEnabled;

    [ObservableProperty]
    private string _searchQuery = string.Empty;

    [ObservableProperty]
    private ObservableCollection<HistoryEntry> _historyEntries = [];

    [ObservableProperty]
    private string _notification = string.Empty;

    [ObservableProperty]
    private bool _isNotificationVisible;

    public HistoryViewModel(
        IHistoryRepository historyRepository,
        IClipboardService clipboardService,
        ISettingsService settingsService)
    {
        _historyRepository = historyRepository;
        _clipboardService = clipboardService;
        _settingsService = settingsService;
        IsHistoryEnabled = _settingsService.CurrentSettings.HistoryEnabled;

        _ = LoadHistoryAsync();
    }

    partial void OnSearchQueryChanged(string value)
    {
        _ = SearchHistoryAsync();
    }

    [RelayCommand]
    public async Task LoadHistoryAsync()
    {
        var items = await _historyRepository.GetAllAsync();
        HistoryEntries = new ObservableCollection<HistoryEntry>(items);
    }

    [RelayCommand]
    public async Task SearchHistoryAsync()
    {
        var items = await _historyRepository.SearchAsync(SearchQuery);
        HistoryEntries = new ObservableCollection<HistoryEntry>(items);
    }

    [RelayCommand]
    public async Task CopyPasswordAsync(HistoryEntry? entry)
    {
        if (entry == null || string.IsNullOrEmpty(entry.Password)) return;

        var settings = _settingsService.CurrentSettings;
        await _clipboardService.CopyToClipboardAsync(
            entry.Password,
            settings.ClipboardAutoClearEnabled,
            settings.ClipboardAutoClearSeconds);

        ShowNotification("Password copied from history!");
    }

    [RelayCommand]
    public async Task DeleteEntryAsync(HistoryEntry? entry)
    {
        if (entry == null) return;

        await _historyRepository.DeleteAsync(entry.Id);
        HistoryEntries.Remove(entry);
        ShowNotification("Entry deleted.");
    }

    [RelayCommand]
    public async Task ClearAllAsync()
    {
        await _historyRepository.ClearAllAsync();
        HistoryEntries.Clear();
        ShowNotification("History cleared completely.");
    }

    [RelayCommand]
    public async Task ToggleHistoryStatusAsync()
    {
        _settingsService.CurrentSettings.HistoryEnabled = IsHistoryEnabled;
        await _settingsService.SaveSettingsAsync();

        if (IsHistoryEnabled)
        {
            ShowNotification("Password History is now ENABLED (Encrypted with DPAPI).");
        }
        else
        {
            ShowNotification("Password History is DISABLED. No new passwords will be saved.");
        }
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
