using FluentAssertions;
using MASA.PasswordGenerator.Application.Analyzers;
using MASA.PasswordGenerator.Application.Generators;
using MASA.PasswordGenerator.Core.Constants;
using MASA.PasswordGenerator.Core.Models;
using Xunit;

namespace MASA.PasswordGenerator.Tests;

public class CryptographicPasswordGeneratorTests
{
    private readonly CryptographicPasswordGenerator _generator;

    public CryptographicPasswordGeneratorTests()
    {
        var entropyCalc = new EntropyCalculator();
        var strengthAnalyzer = new PasswordStrengthAnalyzer(entropyCalc);
        _generator = new CryptographicPasswordGenerator(strengthAnalyzer);
    }

    [Theory]
    [InlineData(8)]
    [InlineData(16)]
    [InlineData(32)]
    [InlineData(64)]
    public void Generate_ShouldReturnExactRequestedLength(int length)
    {
        var options = new PasswordOptions
        {
            Length = length,
            IncludeUppercase = true,
            IncludeLowercase = true,
            IncludeDigits = true,
            IncludeSymbols = true
        };

        var result = _generator.Generate(options);

        result.Value.Should().HaveLength(length);
        result.Length.Should().Be(length);
    }

    [Fact]
    public void Generate_WithAllClasses_ShouldIncludeAllSelectedClasses()
    {
        var options = new PasswordOptions
        {
            Length = 20,
            IncludeUppercase = true,
            IncludeLowercase = true,
            IncludeDigits = true,
            IncludeSymbols = true
        };

        for (int i = 0; i < 50; i++)
        {
            var result = _generator.Generate(options);
            result.Value.Should().MatchRegex("[A-Z]");
            result.Value.Should().MatchRegex("[a-z]");
            result.Value.Should().MatchRegex("[0-9]");
            result.Value.Any(c => !char.IsLetterOrDigit(c)).Should().BeTrue();
        }
    }

    [Fact]
    public void Generate_ExcludingSimilarCharacters_ShouldNeverContainSimilarChars()
    {
        var options = new PasswordOptions
        {
            Length = 30,
            IncludeUppercase = true,
            IncludeLowercase = true,
            IncludeDigits = true,
            IncludeSymbols = true,
            ExcludeSimilarCharacters = true
        };

        for (int i = 0; i < 50; i++)
        {
            var result = _generator.Generate(options);
            foreach (char c in CharacterSets.SimilarCharacters)
            {
                result.Value.Should().NotContain(c.ToString());
            }
        }
    }

    [Fact]
    public void Generate_ExcludingAmbiguousSymbols_ShouldNeverContainAmbiguousSymbols()
    {
        var options = new PasswordOptions
        {
            Length = 30,
            IncludeUppercase = true,
            IncludeLowercase = true,
            IncludeDigits = true,
            IncludeSymbols = true,
            ExcludeAmbiguousSymbols = true
        };

        for (int i = 0; i < 50; i++)
        {
            var result = _generator.Generate(options);
            foreach (char c in CharacterSets.AmbiguousSymbols)
            {
                result.Value.Should().NotContain(c.ToString());
            }
        }
    }

    [Fact]
    public void Generate_CustomCharactersOnly_ShouldStrictlyUseCustomSet()
    {
        string customSet = "ABC123!@";
        var options = new PasswordOptions
        {
            Length = 25,
            CustomCharacters = customSet,
            UseCustomCharactersOnly = true
        };

        var result = _generator.Generate(options);

        result.Value.Should().HaveLength(25);
        result.Value.All(c => customSet.Contains(c)).Should().BeTrue();
    }

    [Fact]
    public void GenerateBulk_ShouldProduceExactRequestedCount()
    {
        var options = new BulkOptions
        {
            Count = 25,
            PasswordOptions = new PasswordOptions { Length = 16 }
        };

        var results = _generator.GenerateBulk(options);

        results.Should().HaveCount(25);
        results.Select(r => r.Value).Distinct().Should().HaveCount(25);
    }
}
