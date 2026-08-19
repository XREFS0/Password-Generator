using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using MASA.PasswordGenerator.App.ViewModels;
using MASA.PasswordGenerator.App.Views;
using MASA.PasswordGenerator.Application.Analyzers;
using MASA.PasswordGenerator.Application.Generators;
using MASA.PasswordGenerator.Application.Policies;
using MASA.PasswordGenerator.Application.Services;
using MASA.PasswordGenerator.Core.Models;
using MASA.PasswordGenerator.Infrastructure.Clipboard;
using MASA.PasswordGenerator.Infrastructure.Security;
using MASA.PasswordGenerator.Infrastructure.Storage;

namespace MASA.PasswordGenerator.App;

public static class ScreenshotExporter
{
    [STAThread]
    public static void CaptureAllScreenshots(string outputDirectory)
    {
        Directory.CreateDirectory(outputDirectory);

        // Setup services
        var entropyCalc = new EntropyCalculator();
        var strengthAnalyzer = new PasswordStrengthAnalyzer(entropyCalc);
        var pwdGen = new CryptographicPasswordGenerator(strengthAnalyzer);
        var passGen = new CryptographicPassphraseGenerator(strengthAnalyzer);
        var pinGen = new PinGenerator(strengthAnalyzer);
        var policyEval = new PolicyEvaluator();
        var secureStorage = new DpapiDataProtector();
        var settingsService = new SettingsService();
        var historyRepo = new SqliteHistoryRepository(secureStorage, settingsService);
        var clipboardService = new SafeClipboardService();
        var breachChecker = new PrivacyPreservingBreachChecker(settingsService);

        // Pre-populate sample history
        _ = historyRepo.AddAsync(new HistoryEntry { Password = "Xk9#mP2$vL8@qR4!", Length = 16, Strength = Core.Enums.PasswordStrength.VeryStrong, EntropyBits = 104.5, GeneratorType = "Standard" });
        _ = historyRepo.AddAsync(new HistoryEntry { Password = "solar-falcon-harvest-94", Length = 24, Strength = Core.Enums.PasswordStrength.Strong, EntropyBits = 78.2, GeneratorType = "Passphrase" });
        _ = historyRepo.AddAsync(new HistoryEntry { Password = "948201", Length = 6, Strength = Core.Enums.PasswordStrength.Weak, EntropyBits = 19.9, GeneratorType = "PIN" });

        var themeService = new Services.WpfThemeService(settingsService);

        var dashVM = new DashboardViewModel(pwdGen, passGen, pinGen, clipboardService, settingsService, historyRepo, _ => { });
        var genVM = new GeneratorViewModel(pwdGen, clipboardService, settingsService, historyRepo);
        var bulkVM = new BulkViewModel(pwdGen, clipboardService, settingsService);
        var passVM = new PassphraseViewModel(passGen, clipboardService, settingsService, historyRepo);
        var checkVM = new CheckerViewModel(strengthAnalyzer, breachChecker, settingsService);
        var histVM = new HistoryViewModel(historyRepo, clipboardService, settingsService);
        var presVM = new PresetsViewModel(policyEval, pwdGen, clipboardService, settingsService);
        var settVM = new SettingsViewModel(settingsService, themeService);
        var abtVM = new AboutViewModel();

        var mainVM = new MainViewModel(dashVM, genVM, bulkVM, passVM, checkVM, histVM, presVM, settVM, abtVM);

        var window = new MainWindow(mainVM)
        {
            Width = 1200,
            Height = 800,
            WindowStyle = WindowStyle.SingleBorderWindow,
            ShowInTaskbar = false
        };

        window.Show();

        // 1. Dashboard
        mainVM.NavigateTo("Dashboard");
        ProcessUiEvents();
        SaveWindowScreenshot(window, Path.Combine(outputDirectory, "01_dashboard.png"));

        // 2. Generator
        mainVM.NavigateTo("Generator");
        ProcessUiEvents();
        SaveWindowScreenshot(window, Path.Combine(outputDirectory, "02_generator.png"));

        // 3. Bulk Generator
        mainVM.NavigateTo("Bulk");
        ProcessUiEvents();
        SaveWindowScreenshot(window, Path.Combine(outputDirectory, "03_bulk_generator.png"));

        // 4. Passphrase Generator
        mainVM.NavigateTo("Passphrase");
        ProcessUiEvents();
        SaveWindowScreenshot(window, Path.Combine(outputDirectory, "04_passphrase.png"));

        // 5. Password Checker
        checkVM.PasswordInput = "M@sa_Secur3_Passw0rd!2026";
        checkVM.Analyze();
        mainVM.NavigateTo("Checker");
        ProcessUiEvents();
        SaveWindowScreenshot(window, Path.Combine(outputDirectory, "05_password_checker.png"));

        // 6. History
        mainVM.NavigateTo("History");
        ProcessUiEvents();
        SaveWindowScreenshot(window, Path.Combine(outputDirectory, "06_history.png"));

        // 7. Presets & Policies
        mainVM.NavigateTo("Presets");
        ProcessUiEvents();
        SaveWindowScreenshot(window, Path.Combine(outputDirectory, "07_presets_and_policies.png"));

        // 8. Settings
        mainVM.NavigateTo("Settings");
        ProcessUiEvents();
        SaveWindowScreenshot(window, Path.Combine(outputDirectory, "08_settings.png"));

        // 9. About
        mainVM.NavigateTo("About");
        ProcessUiEvents();
        SaveWindowScreenshot(window, Path.Combine(outputDirectory, "09_about.png"));

        window.Close();
    }

    private static void ProcessUiEvents()
    {
        var frame = new System.Windows.Threading.DispatcherFrame();
        System.Windows.Threading.Dispatcher.CurrentDispatcher.BeginInvoke(
            System.Windows.Threading.DispatcherPriority.Background,
            new System.Windows.Threading.DispatcherOperationCallback(f =>
            {
                ((System.Windows.Threading.DispatcherFrame)f!).Continue = false;
                return null;
            }), frame);
        System.Windows.Threading.Dispatcher.PushFrame(frame);
        Thread.Sleep(80);
    }

    private static void SaveWindowScreenshot(Window window, string savePath)
    {
        int width = (int)window.ActualWidth;
        int height = (int)window.ActualHeight;

        if (width <= 0) width = 1200;
        if (height <= 0) height = 800;

        var rtb = new RenderTargetBitmap(width, height, 96, 96, PixelFormats.Pbgra32);
        rtb.Render(window);

        var encoder = new PngBitmapEncoder();
        encoder.Frames.Add(BitmapFrame.Create(rtb));

        using var fs = File.Create(savePath);
        encoder.Save(fs);
    }
}
