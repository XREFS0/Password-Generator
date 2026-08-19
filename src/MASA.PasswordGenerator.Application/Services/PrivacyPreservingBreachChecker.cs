using System.Security.Cryptography;
using System.Text;
using MASA.PasswordGenerator.Core.Interfaces;

namespace MASA.PasswordGenerator.Application.Services;

public class PrivacyPreservingBreachChecker : IPasswordBreachChecker
{
    private readonly ISettingsService _settingsService;
    private static readonly HttpClient HttpClient = new();

    public PrivacyPreservingBreachChecker(ISettingsService settingsService)
    {
        _settingsService = settingsService;
    }

    public async Task<BreachCheckResult> CheckBreachAsync(string password, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(password))
        {
            return new BreachCheckResult
            {
                IsBreached = false,
                BreachCount = 0,
                Message = "No password provided to check."
            };
        }

        // Privacy Guard: Breach checking MUST be explicitly opted into by user in Settings
        if (!_settingsService.CurrentSettings.BreachCheckEnabled)
        {
            return new BreachCheckResult
            {
                IsBreached = false,
                BreachCount = 0,
                Message = "Breach checking is disabled in Settings to protect offline privacy."
            };
        }

        try
        {
            // Privacy Architecture: Use k-Anonymity model (HIBP standard model)
            // Compute SHA-1 hash locally
            byte[] inputBytes = Encoding.UTF8.GetBytes(password);
            byte[] hashBytes = SHA1.HashData(inputBytes);
            string hashHex = Convert.ToHexString(hashBytes).ToUpperInvariant();

            // Send ONLY first 5 characters (prefix) over HTTPS
            string prefix = hashHex[..5];
            string suffix = hashHex[5..];

            string url = $"https://api.pwnedpasswords.com/range/{prefix}";
            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Add("User-Agent", "MASA-PasswordGenerator-Desktop-PrivacySafe");

            var response = await HttpClient.SendAsync(request, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return new BreachCheckResult
                {
                    IsBreached = false,
                    BreachCount = 0,
                    Message = $"Remote service returned status code {response.StatusCode}."
                };
            }

            string responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
            using var reader = new StringReader(responseBody);
            string? line;

            while ((line = await reader.ReadLineAsync(cancellationToken)) != null)
            {
                var parts = line.Split(':');
                if (parts.Length == 2 && string.Equals(parts[0].Trim(), suffix, StringComparison.OrdinalIgnoreCase))
                {
                    long count = long.TryParse(parts[1].Trim(), out var parsed) ? parsed : 1;
                    return new BreachCheckResult
                    {
                        IsBreached = true,
                        BreachCount = count,
                        Message = $"WARNING: This password has been exposed in {count:N0} known data breaches! Do NOT use this password."
                    };
                }
            }

            return new BreachCheckResult
            {
                IsBreached = false,
                BreachCount = 0,
                Message = "Safe! No known occurrences found in breach databases using k-Anonymity verification."
            };
        }
        catch (Exception ex)
        {
            return new BreachCheckResult
            {
                IsBreached = false,
                BreachCount = 0,
                Message = $"Privacy-safe check could not connect: {ex.Message}"
            };
        }
    }
}
