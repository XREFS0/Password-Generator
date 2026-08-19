using MASA.PasswordGenerator.Core.Interfaces;

namespace MASA.PasswordGenerator.Application.Analyzers;

public class EntropyCalculator : IEntropyCalculator
{
    public double CalculateEntropy(string password)
    {
        if (string.IsNullOrEmpty(password))
        {
            return 0.0;
        }

        int poolSize = 0;
        bool hasUpper = false;
        bool hasLower = false;
        bool hasDigits = false;
        bool hasStandardSymbols = false;
        bool hasOtherChars = false;

        foreach (char c in password)
        {
            if (char.IsUpper(c)) hasUpper = true;
            else if (char.IsLower(c)) hasLower = true;
            else if (char.IsDigit(c)) hasDigits = true;
            else if (char.IsPunctuation(c) || char.IsSymbol(c)) hasStandardSymbols = true;
            else hasOtherChars = true;
        }

        if (hasUpper) poolSize += 26;
        if (hasLower) poolSize += 26;
        if (hasDigits) poolSize += 10;
        if (hasStandardSymbols) poolSize += 33;
        if (hasOtherChars) poolSize += 30;

        if (poolSize == 0)
        {
            return 0.0;
        }

        // Shannon Entropy / Information Entropy = Length * log2(poolSize)
        double entropy = password.Length * Math.Log2(poolSize);
        return Math.Round(entropy, 1);
    }

    public string GetSecurityDescription(double entropyBits)
    {
        return entropyBits switch
        {
            < 28 => "Very Weak (Can be cracked instantly)",
            < 45 => "Weak (Cracked in minutes or hours)",
            < 65 => "Fair (Moderate resistance to brute force)",
            < 85 => "Strong (Immune to online and standard offline attacks)",
            _ => "Very Strong (Maximum cryptographic protection - computationally infeasible to crack)"
        };
    }
}
