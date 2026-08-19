using FluentAssertions;
using MASA.PasswordGenerator.Application.Analyzers;
using MASA.PasswordGenerator.Application.Generators;
using MASA.PasswordGenerator.Application.Policies;
using MASA.PasswordGenerator.Core.Enums;
using MASA.PasswordGenerator.Core.Models;
using MASA.PasswordGenerator.Infrastructure.Security;
using Xunit;

namespace MASA.PasswordGenerator.Tests;

public class PassphrasePinPolicyAndSecurityTests
{
    private readonly CryptographicPassphraseGenerator _passphraseGenerator;
    private readonly PinGenerator _pinGenerator;
    private readonly PolicyEvaluator _policyEvaluator;
    private readonly DpapiDataProtector _dataProtector;

    public PassphrasePinPolicyAndSecurityTests()
    {
        var entropyCalc = new EntropyCalculator();
        var strengthAnalyzer = new PasswordStrengthAnalyzer(entropyCalc);
        _passphraseGenerator = new CryptographicPassphraseGenerator(strengthAnalyzer);
        _pinGenerator = new PinGenerator(strengthAnalyzer);
        _policyEvaluator = new PolicyEvaluator();
        _dataProtector = new DpapiDataProtector();
    }

    [Theory]
    [InlineData(4, "-")]
    [InlineData(6, "_")]
    [InlineData(5, ".")]
    public void PassphraseGenerator_ShouldGenerateCorrectWordCountAndSeparator(int wordCount, string separator)
    {
        var options = new PassphraseOptions
        {
            WordCount = wordCount,
            Separator = separator,
            IncludeNumber = false
        };

        var result = _passphraseGenerator.Generate(options);
        var split = result.Value.Split(separator);

        split.Should().HaveCount(wordCount);
    }

    [Theory]
    [InlineData(4)]
    [InlineData(6)]
    [InlineData(8)]
    public void PinGenerator_ShouldGenerateExactDigitsOnly(int length)
    {
        var result = _pinGenerator.Generate(new PinOptions { Length = length });

        result.Value.Should().HaveLength(length);
        result.Value.All(char.IsDigit).Should().BeTrue();
    }

    [Fact]
    public void PolicyEvaluator_ShouldValidateWindowsPolicyCorrectly()
    {
        var windowsPolicy = _policyEvaluator.GetBuiltinPolicies().First(p => p.Name.Contains("Windows"));

        var valid = _policyEvaluator.Validate("Password123!@", windowsPolicy);
        valid.IsCompliant.Should().BeTrue();

        var invalidTooShort = _policyEvaluator.Validate("Pass1!", windowsPolicy);
        invalidTooShort.IsCompliant.Should().BeFalse();

        var invalidNoDigits = _policyEvaluator.Validate("Password!!", windowsPolicy);
        invalidNoDigits.IsCompliant.Should().BeFalse();
    }

    [Fact]
    public void Dpapi_RoundTrip_ShouldEncryptAndDecryptAccurately()
    {
        string secretPassword = "SuperSecret_P@ssw0rd_2026";
        string encrypted = _dataProtector.Protect(secretPassword);

        encrypted.Should().NotBeNullOrEmpty();
        encrypted.Should().NotBe(secretPassword);

        string decrypted = _dataProtector.Unprotect(encrypted);
        decrypted.Should().Be(secretPassword);
    }
}
