using System.Text.RegularExpressions;
using MASA.PasswordGenerator.Core.Interfaces;
using MASA.PasswordGenerator.Core.Models;

namespace MASA.PasswordGenerator.Application.Policies;

public class PolicyEvaluator : IPolicyEvaluator
{
    public PolicyValidationResult Validate(string password, PasswordPolicy policy)
    {
        ArgumentNullException.ThrowIfNull(policy);

        var result = new PolicyValidationResult { IsCompliant = true };

        if (string.IsNullOrEmpty(password))
        {
            result.IsCompliant = false;
            result.Errors.Add("Password cannot be empty.");
            return result;
        }

        if (password.Length < policy.MinLength)
        {
            result.IsCompliant = false;
            result.Errors.Add($"Password must be at least {policy.MinLength} characters long.");
        }

        if (password.Length > policy.MaxLength)
        {
            result.IsCompliant = false;
            result.Errors.Add($"Password cannot exceed {policy.MaxLength} characters.");
        }

        if (policy.RequireUppercase && !password.Any(char.IsUpper))
        {
            result.IsCompliant = false;
            result.Errors.Add("Password must contain at least one uppercase letter (A-Z).");
        }

        if (policy.RequireLowercase && !password.Any(char.IsLower))
        {
            result.IsCompliant = false;
            result.Errors.Add("Password must contain at least one lowercase letter (a-z).");
        }

        if (policy.RequireDigit && !password.Any(char.IsDigit))
        {
            result.IsCompliant = false;
            result.Errors.Add("Password must contain at least one digit (0-9).");
        }

        if (policy.RequireSymbol && !password.Any(c => !char.IsLetterOrDigit(c)))
        {
            result.IsCompliant = false;
            result.Errors.Add("Password must contain at least one special symbol.");
        }

        if (policy.MaxConsecutiveRepeats > 0)
        {
            string pattern = $"(.)\\1{{{policy.MaxConsecutiveRepeats},}}";
            if (Regex.IsMatch(password, pattern))
            {
                result.IsCompliant = false;
                result.Errors.Add($"Password cannot contain more than {policy.MaxConsecutiveRepeats} repeated identical characters consecutively.");
            }
        }

        if (!string.IsNullOrEmpty(policy.ForbiddenCharacters))
        {
            var forbidden = new HashSet<char>(policy.ForbiddenCharacters);
            if (password.Any(forbidden.Contains))
            {
                result.IsCompliant = false;
                result.Errors.Add($"Password contains forbidden characters: {policy.ForbiddenCharacters}");
            }
        }

        return result;
    }

    public IReadOnlyList<PasswordPolicy> GetBuiltinPolicies()
    {
        return
        [
            new PasswordPolicy
            {
                Name = "Windows Complexity Policy",
                Description = "Adheres to Windows Active Directory standard password complexity rules.",
                MinLength = 8,
                MaxLength = 128,
                RequireUppercase = true,
                RequireLowercase = true,
                RequireDigit = true,
                RequireSymbol = true,
                MaxConsecutiveRepeats = 2
            },
            new PasswordPolicy
            {
                Name = "High Security (NIST / ISO 27001)",
                Description = "Strict enterprise policy requiring length and symbol diversity.",
                MinLength = 16,
                MaxLength = 128,
                RequireUppercase = true,
                RequireLowercase = true,
                RequireDigit = true,
                RequireSymbol = true,
                MaxConsecutiveRepeats = 1
            },
            new PasswordPolicy
            {
                Name = "Banking & Financial Standard",
                Description = "12+ alphanumeric characters without ambiguous brackets or quotes.",
                MinLength = 12,
                MaxLength = 64,
                RequireUppercase = true,
                RequireLowercase = true,
                RequireDigit = true,
                RequireSymbol = true,
                ForbiddenCharacters = "<>{}`~"
            },
            new PasswordPolicy
            {
                Name = "Basic Alphanumeric (PIN/Code)",
                Description = "Simple letters and numbers without special characters.",
                MinLength = 6,
                MaxLength = 32,
                RequireUppercase = false,
                RequireLowercase = false,
                RequireDigit = true,
                RequireSymbol = false
            }
        ];
    }
}
