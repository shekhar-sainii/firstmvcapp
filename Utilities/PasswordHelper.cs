using System.Security.Cryptography;
using System.Text;

namespace FirstMvcApp.Utilities;

public static class PasswordHelper
{
    /// <summary>
    /// Hash a password using SHA256
    /// </summary>
    public static string HashPassword(string password)
    {
        using (var sha256 = SHA256.Create())
        {
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(hashedBytes);
        }
    }

    /// <summary>
    /// Verify a password against its hash
    /// </summary>
    public static bool VerifyPassword(string password, string hash)
    {
        var hashOfInput = HashPassword(password);
        return hashOfInput.Equals(hash);
    }
}
