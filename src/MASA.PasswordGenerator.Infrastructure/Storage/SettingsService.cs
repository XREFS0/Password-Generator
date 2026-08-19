using System.IO;
using System.Text.Json;
using MASA.PasswordGenerator.Core.Interfaces;

namespace MASA.PasswordGenerator.Infrastructure.Storage;

public class SettingsService : ISettingsService
{
    private readonly string _settingsFilePath;
    private AppSettings _currentSettings = new();

    public AppSettings CurrentSettings => _currentSettings;

    public SettingsService()
    {
        string appDataPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "MASA.PasswordGenerator");

        Directory.CreateDirectory(appDataPath);
        _settingsFilePath = Path.Combine(appDataPath, "settings.json");
    }

    public async Task LoadSettingsAsync()
    {
        try
        {
            if (File.Exists(_settingsFilePath))
            {
                string json = await File.ReadAllTextAsync(_settingsFilePath);
                var loaded = JsonSerializer.Deserialize<AppSettings>(json);
                if (loaded != null)
                {
                    _currentSettings = loaded;
                }
            }
        }
        catch
        {
            _currentSettings = new AppSettings();
        }
    }

    public async Task SaveSettingsAsync()
    {
        try
        {
            string json = JsonSerializer.Serialize(_currentSettings, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(_settingsFilePath, json);
        }
        catch
        {
            // Fail safe on settings write
        }
    }
}
