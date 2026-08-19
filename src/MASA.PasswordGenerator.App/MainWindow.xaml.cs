using System.Windows;
using MASA.PasswordGenerator.App.ViewModels;

namespace MASA.PasswordGenerator.App;

public partial class MainWindow : Window
{
    public MainWindow(MainViewModel mainViewModel)
    {
        InitializeComponent();
        DataContext = mainViewModel;
    }
}