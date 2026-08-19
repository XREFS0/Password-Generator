using MASA.PasswordGenerator.Core.Enums;

namespace MASA.PasswordGenerator.Core.Models;

public class PasswordOptions
{
    public int Length { get; set; } = 16;
    public bool IncludeUppercase { get; set; } = true;
    public bool IncludeLowercase { get; set; } = true;
    public bool IncludeDigits { get; set; } = true;
    public bool IncludeSymbols { get; set; } = true;
    public bool ExcludeSimilarCharacters { get; set; } = false;
    public bool ExcludeAmbiguousSymbols { get; set; } = false;
    public string CustomCharacters { get; set; } = string.Empty;
    public bool UseCustomCharactersOnly { get; set; } = false;

    public static PasswordOptions Simple => new()
    {
        Length = 12,
        IncludeUppercase = true,
        IncludeLowercase = true,
        IncludeDigits = true,
        IncludeSymbols = false
    };

    public static PasswordOptions Strong => new()
    {
        Length = 16,
        IncludeUppercase = true,
        IncludeLowercase = true,
        IncludeDigits = true,
        IncludeSymbols = true
    };

    public static PasswordOptions MaximumSecurity => new()
    {
        Length = 32,
        IncludeUppercase = true,
        IncludeLowercase = true,
        IncludeDigits = true,
        IncludeSymbols = true,
        ExcludeSimilarCharacters = false,
        ExcludeAmbiguousSymbols = false
    };
}

public class PassphraseOptions
{
    public int WordCount { get; set; } = 4;
    public string Separator { get; set; } = "-";
    public PassphraseCasing Casing { get; set; } = PassphraseCasing.Lowercase;
    public bool IncludeNumber { get; set; } = true;
    public bool CapitalizeFirstLetter { get; set; } = false;
}

public class PinOptions
{
    public int Length { get; set; } = 6;
}

public class BulkOptions
{
    public int Count { get; set; } = 10;
    public PasswordOptions PasswordOptions { get; set; } = new();
}

public class StrengthResult
{
    public PasswordStrength Strength { get; set; }
    public double EntropyBits { get; set; }
    public int Score { get; set; } // 0 - 100
    public string StrengthLabel => Strength switch
    {
        PasswordStrength.VeryWeak => "Very Weak",
        PasswordStrength.Weak => "Weak",
        PasswordStrength.Fair => "Fair",
        PasswordStrength.Strong => "Strong",
        PasswordStrength.VeryStrong => "Very Strong",
        _ => "Unknown"
    };
    public string CrackTimeEstimate { get; set; } = string.Empty;
    public List<string> Suggestions { get; set; } = [];
    public List<string> Warnings { get; set; } = [];
    public bool HasUppercase { get; set; }
    public bool HasLowercase { get; set; }
    public bool HasDigits { get; set; }
    public bool HasSymbols { get; set; }
    public int UniqueCharacterCount { get; set; }
}

public class PasswordResult
{
    public string Value { get; set; } = string.Empty;
    public int Length => Value?.Length ?? 0;
    public StrengthResult Strength { get; set; } = new();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class HistoryEntry
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Password { get; set; } = string.Empty;
    public int Length { get; set; }
    public PasswordStrength Strength { get; set; }
    public double EntropyBits { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public string GeneratorType { get; set; } = "Standard";
}

public class CustomPreset
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public PasswordOptions Options { get; set; } = new();
    public bool IsBuiltin { get; set; } = false;
}

public class PasswordPolicy
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int MinLength { get; set; } = 8;
    public int MaxLength { get; set; } = 128;
    public bool RequireUppercase { get; set; } = true;
    public bool RequireLowercase { get; set; } = true;
    public bool RequireDigit { get; set; } = true;
    public bool RequireSymbol { get; set; } = false;
    public int MaxConsecutiveRepeats { get; set; } = 2;
    public string ForbiddenCharacters { get; set; } = string.Empty;
}

public class PolicyValidationResult
{
    public bool IsCompliant { get; set; }
    public List<string> Errors { get; set; } = [];
}
