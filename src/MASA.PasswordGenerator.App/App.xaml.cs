using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using MASA.PasswordGenerator.App.Services;
using MASA.PasswordGenerator.App.ViewModels;
using MASA.PasswordGenerator.Application.Analyzers;
using MASA.PasswordGenerator.Application.Generators;
using MASA.PasswordGenerator.Application.Policies;
using MASA.PasswordGenerator.Application.Services;
using MASA.PasswordGenerator.Core.Interfaces;
using MASA.PasswordGenerator.Infrastructure.Clipboard;
using MASA.PasswordGenerator.Infrastructure.Security;
using MASA.PasswordGenerator.Infrastructure.Storage;

namespace MASA.PasswordGenerator.App;

public partial class App : System.Windows.Application
{
    private ServiceProvider? _serviceProvider;

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        if (e.Args.Length > 0 && e.Args[0] == "--capture-screenshots")
        {
            string screenshotsDir = e.Args.Length > 1 
                ? e.Args[1] 
                : System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Screenshots");
            
            ScreenshotExporter.CaptureAllScreenshots(screenshotsDir);
            Shutdown();
            return;
        }

        var serviceCollection = new ServiceCollection();
        ConfigureServices(serviceCollection);

        _serviceProvider = serviceCollection.BuildServiceProvider();

        var settingsService = _serviceProvider.GetRequiredService<ISettingsService>();
        await settingsService.LoadSettingsAsync();

        var themeService = _serviceProvider.GetRequiredService<WpfThemeService>();
        themeService.ApplyTheme(settingsService.CurrentSettings.Theme);

        var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
        mainWindow.Show();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _serviceProvider?.Dispose();
        base.OnExit(e);
    }

    private static void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<IEntropyCalculator, EntropyCalculator>();
        services.AddSingleton<IStrengthAnalyzer, PasswordStrengthAnalyzer>();
        services.AddSingleton<IPasswordGenerator, CryptographicPasswordGenerator>();
        services.AddSingleton<IPassphraseGenerator, CryptographicPassphraseGenerator>();
        services.AddSingleton<IPinGenerator, PinGenerator>();
        services.AddSingleton<IPolicyEvaluator, PolicyEvaluator>();
        services.AddSingleton<IPasswordBreachChecker, PrivacyPreservingBreachChecker>();

        services.AddSingleton<ISecureStorage, DpapiDataProtector>();
        services.AddSingleton<ISettingsService, SettingsService>();
        services.AddSingleton<IHistoryRepository, SqliteHistoryRepository>();
        services.AddSingleton<IClipboardService, SafeClipboardService>();

        services.AddSingleton<WpfThemeService>();
        services.AddSingleton<DashboardViewModel>(sp => new DashboardViewModel(
            sp.GetRequiredService<IPasswordGenerator>(),
            sp.GetRequiredService<IPassphraseGenerator>(),
            sp.GetRequiredService<IPinGenerator>(),
            sp.GetRequiredService<IClipboardService>(),
            sp.GetRequiredService<ISettingsService>(),
            sp.GetRequiredService<IHistoryRepository>(),
            target => sp.GetRequiredService<MainViewModel>().NavigateTo(target)));

        services.AddSingleton<GeneratorViewModel>();
        services.AddSingleton<BulkViewModel>();
        services.AddSingleton<PassphraseViewModel>();
        services.AddSingleton<CheckerViewModel>();
        services.AddSingleton<HistoryViewModel>();
        services.AddSingleton<PresetsViewModel>();
        services.AddSingleton<SettingsViewModel>();
        services.AddSingleton<AboutViewModel>();
        services.AddSingleton<MainViewModel>();

        services.AddSingleton<MainWindow>();
    }
}
