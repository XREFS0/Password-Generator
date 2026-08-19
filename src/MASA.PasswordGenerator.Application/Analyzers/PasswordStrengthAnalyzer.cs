using System.Text.RegularExpressions;
using MASA.PasswordGenerator.Core.Enums;
using MASA.PasswordGenerator.Core.Interfaces;
using MASA.PasswordGenerator.Core.Models;

namespace MASA.PasswordGenerator.Application.Analyzers;

public class PasswordStrengthAnalyzer : IStrengthAnalyzer
{
    private readonly IEntropyCalculator _entropyCalculator;

    private static readonly string[] CommonDictionaryPasswords =
    [
        "password", "123456", "12345678", "123456789", "qwerty", "12345", "1234", "111111",
        "1234567", "dragon", "123123", "baseball", "football", "welcome", "admin", "administrator",
        "master", "monkey", "shadow", "sunshine", "princess", "iloveyou", "secret", "letmein"
    ];

    public PasswordStrengthAnalyzer(IEntropyCalculator entropyCalculator)
    {
        _entropyCalculator = entropyCalculator;
    }

    public StrengthResult Analyze(string password)
    {
        if (string.IsNullOrEmpty(password))
        {
            return new StrengthResult
            {
                Strength = PasswordStrength.VeryWeak,
                EntropyBits = 0,
                Score = 0,
                CrackTimeEstimate = "0 seconds",
                Suggestions = ["Enter a password to analyze."]
            };
        }

        var result = new StrengthResult();
        int score = 0;

        // Character classes inspection
        foreach (char c in password)
        {
            if (char.IsUpper(c)) result.HasUppercase = true;
            else if (char.IsLower(c)) result.HasLowercase = true;
            else if (char.IsDigit(c)) result.HasDigits = true;
            else result.HasSymbols = true;
        }

        result.UniqueCharacterCount = password.Distinct().Count();
        result.EntropyBits = _entropyCalculator.CalculateEntropy(password);

        // 1. Length scoring
        int length = password.Length;
        if (length < 6) score += 5;
        else if (length < 8) score += 15;
        else if (length < 12) score += 30;
        else if (length < 16) score += 40;
        else if (length < 24) score += 55;
        else score += 65;

        // 2. Variety scoring
        int varietyCount = 0;
        if (result.HasUppercase) varietyCount++;
        if (result.HasLowercase) varietyCount++;
        if (result.HasDigits) varietyCount++;
        if (result.HasSymbols) varietyCount++;

        score += varietyCount switch
        {
            1 => 5,
            2 => 15,
            3 => 25,
            4 => 35,
            _ => 0
        };

        // 3. Uniqueness bonus / penalty
        double uniquenessRatio = (double)result.UniqueCharacterCount / length;
        if (uniquenessRatio < 0.5)
        {
            score -= 15;
            result.Warnings.Add("Low variety: Many repeating characters detected.");
        }
        else if (uniquenessRatio > 0.8 && length >= 12)
        {
            score += 10;
        }

        // 4. Sequential detection (e.g. 1234, abcd)
        if (ContainsSequentialCharacters(password, 4))
        {
            score -= 15;
            result.Warnings.Add("Contains sequential numbers or letters (e.g. '1234' or 'abcd').");
        }

        // 5. Common Dictionary match
        string lower = password.ToLowerInvariant();
        if (CommonDictionaryPasswords.Any(common => lower.Contains(common)))
        {
            score -= 40;
            result.Warnings.Add("Contains very common dictionary words or predictable patterns.");
        }

        // 6. Repeated consecutive characters (e.g. 'aaaa', '1111')
        if (Regex.IsMatch(password, @"(.)\1{2,}"))
        {
            score -= 15;
            result.Warnings.Add("Contains 3 or more identical characters in a row.");
        }

        // Clamp score 0 to 100
        score = Math.Clamp(score, 0, 100);
        result.Score = score;

        // Determine Strength Category
        result.Strength = score switch
        {
            < 25 => PasswordStrength.VeryWeak,
            < 50 => PasswordStrength.Weak,
            < 70 => PasswordStrength.Fair,
            < 90 => PasswordStrength.Strong,
            _ => PasswordStrength.VeryStrong
        };

        // Suggestions based on findings
        if (length < 12)
        {
            result.Suggestions.Add("Make the password at least 12 characters long.");
        }
        if (!result.HasUppercase)
        {
            result.Suggestions.Add("Add uppercase letters (A-Z).");
        }
        if (!result.HasLowercase)
        {
            result.Suggestions.Add("Add lowercase letters (a-z).");
        }
        if (!result.HasDigits)
        {
            result.Suggestions.Add("Add numbers (0-9).");
        }
        if (!result.HasSymbols)
        {
            result.Suggestions.Add("Add special symbols (!@#$%).");
        }
        if (result.Suggestions.Count == 0 && result.Warnings.Count == 0)
        {
            result.Suggestions.Add("Excellent! This password is well-constructed and highly resilient.");
        }

        result.CrackTimeEstimate = EstimateCrackTime(result.EntropyBits);

        return result;
    }

    private static bool ContainsSequentialCharacters(string password, int minSequenceLength)
    {
        if (password.Length < minSequenceLength) return false;

        for (int i = 0; i <= password.Length - minSequenceLength; i++)
        {
            bool isForwardAsc = true;
            bool isBackwardAsc = true;

            for (int j = 0; j < minSequenceLength - 1; j++)
            {
                if (password[i + j + 1] - password[i + j] != 1) isForwardAsc = false;
                if (password[i + j] - password[i + j + 1] != 1) isBackwardAsc = false;
            }

            if (isForwardAsc || isBackwardAsc) return true;
        }

        return false;
    }

    private static string EstimateCrackTime(double entropy)
    {
        return entropy switch
        {
            < 20 => "Instant (< 1 second)",
            < 30 => "A few seconds to minutes",
            < 45 => "A few hours to days",
            < 60 => "Several months to a few years",
            < 75 => "Hundreds of years",
            < 90 => "Millions of years",
            _ => "Trillions of centuries (Virtually impossible)"
        };
    }
}
