using System.Security.Cryptography;
using System.Text;
using MASA.PasswordGenerator.Core.Constants;
using MASA.PasswordGenerator.Core.Interfaces;
using MASA.PasswordGenerator.Core.Models;

namespace MASA.PasswordGenerator.Application.Generators;

public class CryptographicPasswordGenerator : IPasswordGenerator
{
    private readonly IStrengthAnalyzer _strengthAnalyzer;

    public CryptographicPasswordGenerator(IStrengthAnalyzer strengthAnalyzer)
    {
        _strengthAnalyzer = strengthAnalyzer;
    }

    public PasswordResult Generate(PasswordOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        if (options.Length < 1)
        {
            throw new ArgumentException("Password length must be at least 1.", nameof(options));
        }

        string characterPool;
        List<string> mandatorySets = [];

        if (options.UseCustomCharactersOnly)
        {
            if (string.IsNullOrEmpty(options.CustomCharacters))
            {
                throw new InvalidOperationException("Custom characters cannot be empty when custom characters only is enabled.");
            }
            characterPool = options.CustomCharacters;
        }
        else
        {
            var poolBuilder = new StringBuilder();

            var upper = CharacterSets.Uppercase;
            var lower = CharacterSets.Lowercase;
            var digits = CharacterSets.Digits;
            var symbols = options.ExcludeAmbiguousSymbols
                ? FilterOutCharacters(CharacterSets.StandardSymbols, CharacterSets.AmbiguousSymbols)
                : CharacterSets.StandardSymbols;

            if (options.ExcludeSimilarCharacters)
            {
                upper = FilterOutCharacters(upper, CharacterSets.SimilarCharacters);
                lower = FilterOutCharacters(lower, CharacterSets.SimilarCharacters);
                digits = FilterOutCharacters(digits, CharacterSets.SimilarCharacters);
                symbols = FilterOutCharacters(symbols, CharacterSets.SimilarCharacters);
            }

            if (options.IncludeUppercase && !string.IsNullOrEmpty(upper))
            {
                poolBuilder.Append(upper);
                mandatorySets.Add(upper);
            }

            if (options.IncludeLowercase && !string.IsNullOrEmpty(lower))
            {
                poolBuilder.Append(lower);
                mandatorySets.Add(lower);
            }

            if (options.IncludeDigits && !string.IsNullOrEmpty(digits))
            {
                poolBuilder.Append(digits);
                mandatorySets.Add(digits);
            }

            if (options.IncludeSymbols && !string.IsNullOrEmpty(symbols))
            {
                poolBuilder.Append(symbols);
                mandatorySets.Add(symbols);
            }

            if (!string.IsNullOrEmpty(options.CustomCharacters))
            {
                var custom = options.ExcludeSimilarCharacters
                    ? FilterOutCharacters(options.CustomCharacters, CharacterSets.SimilarCharacters)
                    : options.CustomCharacters;

                if (!string.IsNullOrEmpty(custom))
                {
                    poolBuilder.Append(custom);
                }
            }

            characterPool = poolBuilder.ToString();
        }

        // Deduplicate characters from pool
        var distinctPool = new string(characterPool.Distinct().ToArray());

        if (string.IsNullOrEmpty(distinctPool))
        {
            throw new InvalidOperationException("No valid characters available with the chosen options. Please select at least one character type.");
        }

        var resultChars = new char[options.Length];
        int currentIndex = 0;

        // Ensure at least one character from each selected mandatory set if length allows
        if (!options.UseCustomCharactersOnly && options.Length >= mandatorySets.Count)
        {
            foreach (var set in mandatorySets)
            {
                var distinctSet = new string(set.Distinct().ToArray());
                if (distinctSet.Length > 0 && currentIndex < options.Length)
                {
                    int randomIndex = RandomNumberGenerator.GetInt32(0, distinctSet.Length);
                    resultChars[currentIndex++] = distinctSet[randomIndex];
                }
            }
        }

        // Fill remaining slots from the general pool
        while (currentIndex < options.Length)
        {
            int randomIndex = RandomNumberGenerator.GetInt32(0, distinctPool.Length);
            resultChars[currentIndex++] = distinctPool[randomIndex];
        }

        // Secure Fisher-Yates shuffle using Cryptographic RNG
        for (int i = resultChars.Length - 1; i > 0; i--)
        {
            int j = RandomNumberGenerator.GetInt32(0, i + 1);
            (resultChars[i], resultChars[j]) = (resultChars[j], resultChars[i]);
        }

        var password = new string(resultChars);
        var strength = _strengthAnalyzer.Analyze(password);

        return new PasswordResult
        {
            Value = password,
            Strength = strength,
            CreatedAt = DateTime.UtcNow
        };
    }

    public IReadOnlyList<PasswordResult> GenerateBulk(BulkOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        int count = Math.Clamp(options.Count, 1, 1000);
        var list = new List<PasswordResult>(count);

        for (int i = 0; i < count; i++)
        {
            list.Add(Generate(options.PasswordOptions));
        }

        return list;
    }

    private static string FilterOutCharacters(string source, string excludeChars)
    {
        var excludeSet = new HashSet<char>(excludeChars);
        return new string(source.Where(c => !excludeSet.Contains(c)).ToArray());
    }
}
