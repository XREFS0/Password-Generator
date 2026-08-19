namespace MASA.PasswordGenerator.Core.Enums;

public enum PasswordStrength
{
    VeryWeak = 0,
    Weak = 1,
    Fair = 2,
    Strong = 3,
    VeryStrong = 4
}

public enum PresetType
{
    Simple,
    Strong,
    MaximumSecurity,
    Pin4,
    Pin6,
    Pin8,
    Passphrase,
    Custom
}

public enum ThemeMode
{
    Dark,
    Light,
    System
}

public enum PassphraseCasing
{
    Lowercase,
    Uppercase,
    TitleCase
}
