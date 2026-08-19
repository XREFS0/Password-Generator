using System.Windows;
using System.Windows.Controls;
using MASA.PasswordGenerator.App.ViewModels;

namespace MASA.PasswordGenerator.App.Views;

public partial class CheckerView : UserControl
{
    private bool _isUpdatingInternally;

    public CheckerView()
    {
        InitializeComponent();
        DataContextChanged += OnDataContextChanged;
    }

    private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (e.NewValue is CheckerViewModel vm)
        {
            if (pwdBox.Password != vm.PasswordInput)
            {
                _isUpdatingInternally = true;
                pwdBox.Password = vm.PasswordInput;
                _isUpdatingInternally = false;
            }
        }
    }

    private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
    {
        if (_isUpdatingInternally) return;

        if (DataContext is CheckerViewModel vm && sender is PasswordBox pwdBoxControl)
        {
            if (vm.PasswordInput != pwdBoxControl.Password)
            {
                vm.PasswordInput = pwdBoxControl.Password;
            }
        }
    }
}
