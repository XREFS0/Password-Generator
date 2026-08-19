using CommunityToolkit.Mvvm.ComponentModel;

namespace MASA.PasswordGenerator.App.ViewModels;

public class AboutViewModel : ObservableObject
{
    public string AppName => "MASA Password Generator";
    public string Version => "1.0.0 (Enterprise Edition)";
    public string Architecture => "Clean Architecture + MVVM + C# .NET 8";
    public string SecurityEngine => "System.Security.Cryptography.RandomNumberGenerator (CSPRNG)";
    public string StorageSecurity => "Windows DPAPI (Data Protection API) + SQLite";
    public string PrivacyPolicy => "100% Offline by default. No passwords sent to any external server. Zero telemetry on sensitive credentials.";
    public string Developer => "xrefs0";
}
