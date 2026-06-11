using System.Security.Cryptography;
using System.Text;

namespace Tenant.Identity.Api.Infrastructure.Authentication;

public sealed class Sha256PasswordHasher : IPasswordHasher
{
    public string Hash(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
        {
            throw new ArgumentException("Password zorunludur.", nameof(password));
        }

        var bytes = Encoding.UTF8.GetBytes(password);
        var hashBytes = SHA256.HashData(bytes);
        return Convert.ToBase64String(hashBytes);
    }

    public bool Verify(string password, string hash)
    {
        return string.Equals(Hash(password), hash, StringComparison.Ordinal);
    }
}