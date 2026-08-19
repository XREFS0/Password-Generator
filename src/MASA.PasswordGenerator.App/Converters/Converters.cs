using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using MASA.PasswordGenerator.Core.Enums;

namespace MASA.PasswordGenerator.App.Converters;

public class StrengthToColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is PasswordStrength strength)
        {
            return strength switch
            {
                PasswordStrength.VeryWeak => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#EF4444")),
                PasswordStrength.Weak => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F97316")),
                PasswordStrength.Fair => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F59E0B")),
                PasswordStrength.Strong => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#10B981")),
                PasswordStrength.VeryStrong => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#06B6D4")),
                _ => new SolidColorBrush(Colors.Gray)
            };
        }

        return new SolidColorBrush(Colors.Gray);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}

public class StrengthToProgressConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is PasswordStrength strength)
        {
            return strength switch
            {
                PasswordStrength.VeryWeak => 20,
                PasswordStrength.Weak => 40,
                PasswordStrength.Fair => 60,
                PasswordStrength.Strong => 80,
                PasswordStrength.VeryStrong => 100,
                _ => 0
            };
        }

        return 0;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}

public class BooleanToVisibilityConverter : IValueConverter
{
    public bool Invert { get; set; } = false;

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        bool flag = value is bool b && b;
        
        if (parameter is string paramStr && string.Equals(paramStr, "invert", StringComparison.OrdinalIgnoreCase))
        {
            flag = !flag;
        }
        else if (Invert)
        {
            flag = !flag;
        }

        return flag ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}

public class NullToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value != null && !string.IsNullOrEmpty(value.ToString()) ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
