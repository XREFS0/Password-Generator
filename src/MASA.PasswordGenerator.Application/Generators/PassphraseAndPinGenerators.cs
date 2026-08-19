using System.Security.Cryptography;
using System.Text;
using MASA.PasswordGenerator.Core.Constants;
using MASA.PasswordGenerator.Core.Enums;
using MASA.PasswordGenerator.Core.Interfaces;
using MASA.PasswordGenerator.Core.Models;

namespace MASA.PasswordGenerator.Application.Generators;

public class CryptographicPassphraseGenerator : IPassphraseGenerator
{
    private readonly IStrengthAnalyzer _strengthAnalyzer;
    private readonly string[] _wordList;

    public CryptographicPassphraseGenerator(IStrengthAnalyzer strengthAnalyzer, string[]? customWordList = null)
    {
        _strengthAnalyzer = strengthAnalyzer;
        _wordList = customWordList is { Length: > 10 } ? customWordList : CharacterSets.DefaultWordList;
    }

    public PasswordResult Generate(PassphraseOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        int wordCount = Math.Clamp(options.WordCount, 2, 20);
        var selectedWords = new List<string>(wordCount);

        for (int i = 0; i < wordCount; i++)
        {
            int index = RandomNumberGenerator.GetInt32(0, _wordList.Length);
            string word = _wordList[index];

            word = options.Casing switch
            {
                PassphraseCasing.Uppercase => word.ToUpperInvariant(),
                PassphraseCasing.TitleCase => char.ToUpperInvariant(word[0]) + word[1..].ToLowerInvariant(),
                _ => word.ToLowerInvariant()
            };

            if (options.CapitalizeFirstLetter && options.Casing == PassphraseCasing.Lowercase && i == 0)
            {
                word = char.ToUpperInvariant(word[0]) + word[1..];
            }

            selectedWords.Add(word);
        }

        if (options.IncludeNumber)
        {
            int randomDigit = RandomNumberGenerator.GetInt32(0, 100);
            int insertIndex = RandomNumberGenerator.GetInt32(0, selectedWords.Count);
            selectedWords[insertIndex] += randomDigit.ToString();
        }

        string passphrase = string.Join(options.Separator, selectedWords);
        var strength = _strengthAnalyzer.Analyze(passphrase);

        return new PasswordResult
        {
            Value = passphrase,
            Strength = strength,
            CreatedAt = DateTime.UtcNow
        };
    }
}

public class PinGenerator : IPinGenerator
{
    private readonly IStrengthAnalyzer _strengthAnalyzer;

    public PinGenerator(IStrengthAnalyzer strengthAnalyzer)
    {
        _strengthAnalyzer = strengthAnalyzer;
    }

    public PasswordResult Generate(PinOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        int length = Math.Clamp(options.Length, 3, 32);
        var sb = new StringBuilder(length);

        for (int i = 0; i < length; i++)
        {
            int digit = RandomNumberGenerator.GetInt32(0, 10);
            sb.Append(digit);
        }

        string pin = sb.ToString();
        var strength = _strengthAnalyzer.Analyze(pin);

        return new PasswordResult
        {
            Value = pin,
            Strength = strength,
            CreatedAt = DateTime.UtcNow
        };
    }
}
