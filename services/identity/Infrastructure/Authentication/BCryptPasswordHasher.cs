namespace Tenant.Identity.Api.Infrastructure.Authentication;

public sealed class BCryptPasswordHasher : IPasswordHasher
{
    public string Hash(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
        {
            throw new ArgumentException("Password zorunludur.", nameof(password));
        }

        return BCrypt.Net.BCrypt.HashPassword(password.Trim(), workFactor: 12);
    }

    public bool Verify(string password, string hash)
    {
        if (string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(hash))
        {
            return false;
        }

        return BCrypt.Net.BCrypt.Verify(password.Trim(), hash);
    }
}