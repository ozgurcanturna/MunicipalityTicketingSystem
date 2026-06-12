using SharedKernel.Domain.Entities;
using Tenant.Identity.Api.Domain.Constants;

namespace Tenant.Identity.Api.Domain.Entities;

public sealed class User : Entity
{
    public Guid TenantId { get; private set; }
    public string Email { get; private set; } = string.Empty;
    public string FullName { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public string Role { get; private set; } = string.Empty;
    public bool IsActive { get; private set; } = true;
    public DateTime? LastLoginAt { get; private set; }

    private User()
    {
    }

    private User(Guid id, Guid tenantId, string email, string fullName, string passwordHash, string role)
    {
        Id = id;
        TenantId = tenantId;
        Email = email;
        FullName = fullName;
        PasswordHash = passwordHash;
        Role = role;
    }

    public static User Create(Guid tenantId, string email, string fullName, string passwordHash, string role, Guid? id = null)
    {
        if (tenantId == Guid.Empty)
        {
            throw new ArgumentException("TenantId zorunludur.", nameof(tenantId));
        }

        if (string.IsNullOrWhiteSpace(email))
        {
            throw new ArgumentException("E-posta zorunludur.", nameof(email));
        }

        if (string.IsNullOrWhiteSpace(fullName))
        {
            throw new ArgumentException("Ad soyad zorunludur.", nameof(fullName));
        }

        if (string.IsNullOrWhiteSpace(passwordHash))
        {
            throw new ArgumentException("PasswordHash zorunludur.", nameof(passwordHash));
        }

        if (string.IsNullOrWhiteSpace(role))
        {
            throw new ArgumentException("Role zorunludur.", nameof(role));
        }

        var normalizedRole = role.Trim().ToUpperInvariant();

        if (!IdentityRoles.IsSupported(normalizedRole))
        {
            throw new ArgumentException($"Desteklenmeyen rol: {normalizedRole}", nameof(role));
        }

        return new User(id ?? Guid.NewGuid(), tenantId, email, fullName, passwordHash, normalizedRole);
    }

    public void RecordLogin()
    {
        LastLoginAt = DateTime.UtcNow;
        MarkAsModified();
    }

    public void Deactivate()
    {
        if (!IsActive)
        {
            return;
        }

        IsActive = false;
        MarkAsModified();
    }

    public bool HasPasswordHash(string passwordHash)
    {
        return string.Equals(PasswordHash, passwordHash, StringComparison.Ordinal);
    }
}