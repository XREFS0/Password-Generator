using MASA.PasswordGenerator.Core.Models;
using MASA.PasswordGenerator.Core.Enums;

namespace MASA.PasswordGenerator.Core.Interfaces;

public interface IPasswordGenerator
{
    PasswordResult Generate(PasswordOptions options);
    IReadOnlyList<PasswordResult> GenerateBulk(BulkOptions options);
}

public interface IPassphraseGenerator
{
    PasswordResult Generate(PassphraseOptions options);
}

public interface IPinGenerator
{
    PasswordResult Generate(PinOptions options);
}

public interface IStrengthAnalyzer
{
    StrengthResult Analyze(string password);
}

public interface IEntropyCalculator
{
    double CalculateEntropy(string password);
    string GetSecurityDescription(double entropyBits);
}

public interface IPolicyEvaluator
{
    PolicyValidationResult Validate(string password, PasswordPolicy policy);
    IReadOnlyList<PasswordPolicy> GetBuiltinPolicies();
}

public interface ISecureStorage
{
    string Protect(string plainText);
    string Unprotect(string protectedText);
}

public interface IHistoryRepository
{
    Task<IReadOnlyList<HistoryEntry>> GetAllAsync(CancellationToken cancellationToken = default);
    Task AddAsync(HistoryEntry entry, CancellationToken cancellationToken = default);
    Task DeleteAsync(string id, CancellationToken cancellationToken = default);
    Task ClearAllAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<HistoryEntry>> SearchAsync(string query, CancellationToken cancellationToken = default);
}

public interface IClipboardService
{
    Task CopyToClipboardAsync(string text, bool autoClear, int autoClearSeconds = 30);
    void ClearClipboard();
}

public interface ISettingsService
{
    AppSettings CurrentSettings { get; }
    Task LoadSettingsAsync();
    Task SaveSettingsAsync();
}

public interface IPasswordBreachChecker
{
    Task<BreachCheckResult> CheckBreachAsync(string password, CancellationToken cancellationToken = default);
}

public class BreachCheckResult
{
    public bool IsBreached { get; set; }
    public long BreachCount { get; set; }
    public string Message { get; set; } = string.Empty;
}

public class AppSettings
{
    public ThemeMode Theme { get; set; } = ThemeMode.Dark;
    public int DefaultLength { get; set; } = 16;
    public bool DefaultUppercase { get; set; } = true;
    public bool DefaultLowercase { get; set; } = true;
    public bool DefaultDigits { get; set; } = true;
    public bool DefaultSymbols { get; set; } = true;
    public bool DefaultExcludeSimilar { get; set; } = false;
    public bool DefaultExcludeAmbiguous { get; set; } = false;
    public PresetType DefaultPreset { get; set; } = PresetType.Strong;
    public bool ClipboardAutoClearEnabled { get; set; } = true;
    public int ClipboardAutoClearSeconds { get; set; } = 30;
    public bool HistoryEnabled { get; set; } = false; // strictly false by default
    public bool BreachCheckEnabled { get; set; } = false; // strictly false by default
    public string Language { get; set; } = "en-US";
}
