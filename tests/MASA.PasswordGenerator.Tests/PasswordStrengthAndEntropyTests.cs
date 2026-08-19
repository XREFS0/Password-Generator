using FluentAssertions;
using MASA.PasswordGenerator.Application.Analyzers;
using MASA.PasswordGenerator.Core.Enums;
using Xunit;

namespace MASA.PasswordGenerator.Tests;

public class PasswordStrengthAndEntropyTests
{
    private readonly PasswordStrengthAnalyzer _analyzer;
    private readonly EntropyCalculator _entropyCalculator;

    public PasswordStrengthAndEntropyTests()
    {
        _entropyCalculator = new EntropyCalculator();
        _analyzer = new PasswordStrengthAnalyzer(_entropyCalculator);
    }

    [Theory]
    [InlineData("123456", PasswordStrength.VeryWeak)]
    [InlineData("password", PasswordStrength.VeryWeak)]
    [InlineData("qwerty", PasswordStrength.VeryWeak)]
    [InlineData("aaaaaa", PasswordStrength.VeryWeak)]
    [InlineData("Abc12345", PasswordStrength.VeryWeak)]
    [InlineData("PassW0rd!", PasswordStrength.Fair)]
    [InlineData("K9#mQ2$xP8@v", PasswordStrength.Strong)]
    [InlineData("T8!yU7@wE4#rQ1$mZ9*kL3&vX6^b", PasswordStrength.VeryStrong)]
    public void Analyze_KnownPasswords_ShouldCategorizeCorrectly(string password, PasswordStrength expectedStrength)
    {
        var result = _analyzer.Analyze(password);
        result.Strength.Should().Be(expectedStrength);
    }

    [Fact]
    public void CalculateEntropy_EmptyPassword_ShouldReturnZero()
    {
        double entropy = _entropyCalculator.CalculateEntropy(string.Empty);
        entropy.Should().Be(0.0);
    }

    [Fact]
    public void CalculateEntropy_Standard16CharComplexPassword_ShouldBeHigherThan70Bits()
    {
        // 16 chars from 95 possible symbols ~ 16 * log2(95) = ~105 bits
        double entropy = _entropyCalculator.CalculateEntropy("A1#bC2$dE3%fG4&h");
        entropy.Should().BeGreaterThan(70.0);
    }
}
