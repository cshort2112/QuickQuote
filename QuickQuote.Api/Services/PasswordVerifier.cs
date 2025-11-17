using System.Security.Cryptography;

namespace QuickQuote.Api.Services;

public class PasswordVerifier
{
    public static bool VerifyPassword(
        string inputPassword,
        string storedHashBase64,
        string storedSaltBase64,
        int iterations)
    {
        byte[] salt = Convert.FromBase64String(storedSaltBase64);
        byte[] storedHash = Convert.FromBase64String(storedHashBase64);

        using var pbkdf2 = new Rfc2898DeriveBytes(
            inputPassword,
            salt,
            iterations,
            HashAlgorithmName.SHA256);
        byte[] computedHash = pbkdf2.GetBytes(storedHash.Length);
        
        return CryptographicOperations.FixedTimeEquals(computedHash, storedHash);
    }
}