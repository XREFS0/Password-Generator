using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MASA.PasswordGenerator.Core.Interfaces;

namespace MASA.PasswordGenerator.App.ViewModels;

public partial class MainViewModel : ObservableObject
{
    [ObservableProperty]
    private object _currentViewModel;

    [ObservableProperty]
    private string _currentViewTitle = "Dashboard";

    public DashboardViewModel DashboardVM { get; }
    public GeneratorViewModel GeneratorVM { get; }
    public BulkViewModel BulkVM { get; }
    public PassphraseViewModel PassphraseVM { get; }
    public CheckerViewModel CheckerVM { get; }
    public HistoryViewModel HistoryVM { get; }
    public PresetsViewModel PresetsVM { get; }
    public SettingsViewModel SettingsVM { get; }
    public AboutViewModel AboutVM { get; }

    public MainViewModel(
        DashboardViewModel dashboardVM,
        GeneratorViewModel generatorVM,
        BulkViewModel bulkVM,
        PassphraseViewModel passphraseVM,
        CheckerViewModel checkerVM,
        HistoryViewModel historyVM,
        PresetsViewModel presetsVM,
        SettingsViewModel settingsVM,
        AboutViewModel aboutVM)
    {
        DashboardVM = dashboardVM;
        GeneratorVM = generatorVM;
        BulkVM = bulkVM;
        PassphraseVM = passphraseVM;
        CheckerVM = checkerVM;
        HistoryVM = historyVM;
        PresetsVM = presetsVM;
        SettingsVM = settingsVM;
        AboutVM = aboutVM;

        _currentViewModel = dashboardVM;
    }

    [RelayCommand]
    public void NavigateTo(string viewName)
    {
        switch (viewName)
        {
            case "Dashboard":
                CurrentViewModel = DashboardVM;
                CurrentViewTitle = "Dashboard";
                break;
            case "Generator":
                CurrentViewModel = GeneratorVM;
                CurrentViewTitle = "Generate Password";
                break;
            case "Bulk":
                CurrentViewModel = BulkVM;
                CurrentViewTitle = "Bulk Password Generator";
                break;
            case "Passphrase":
                CurrentViewModel = PassphraseVM;
                CurrentViewTitle = "Passphrase Generator";
                break;
            case "Checker":
                CurrentViewModel = CheckerVM;
                CurrentViewTitle = "Password Checker";
                break;
            case "History":
                CurrentViewModel = HistoryVM;
                CurrentViewTitle = "Password History";
                _ = HistoryVM.LoadHistoryAsync();
                break;
            case "Presets":
                CurrentViewModel = PresetsVM;
                CurrentViewTitle = "Presets & Policies";
                break;
            case "Settings":
                CurrentViewModel = SettingsVM;
                CurrentViewTitle = "Settings";
                break;
            case "About":
                CurrentViewModel = AboutVM;
                CurrentViewTitle = "About MASA";
                break;
        }
    }
}
