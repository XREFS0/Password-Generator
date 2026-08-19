using System.Security.Cryptography;
using System.Text;
using MASA.PasswordGenerator.Core.Interfaces;

namespace MASA.PasswordGenerator.Infrastructure.Security;

public class DpapiDataProtector : ISecureStorage
{
    private static readonly byte[] Entropy = "MASA_SECURE_STORAGE_SALT_2026"u8.ToArray();

    public string Protect(string plainText)
    {
        if (string.IsNullOrEmpty(plainText))
        {
            return string.Empty;
        }

        byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);
        byte[] protectedBytes = ProtectedData.Protect(
            plainBytes,
            Entropy,
            DataProtectionScope.CurrentUser);

        return Convert.ToBase64String(protectedBytes);
    }

    public string Unprotect(string protectedText)
    {
        if (string.IsNullOrEmpty(protectedText))
        {
            return string.Empty;
        }

        try
        {
            byte[] protectedBytes = Convert.FromBase64String(protectedText);
            byte[] plainBytes = ProtectedData.Unprotect(
                protectedBytes,
                Entropy,
                DataProtectionScope.CurrentUser);

            return Encoding.UTF8.GetString(plainBytes);
        }
        catch
        {
            return string.Empty;
        }
    }
}
