using SharedKernel.Domain.Entities;

namespace Tenant.Identity.Api.Domain.Entities;

public sealed class User : Entity
{
    public Guid TenantId { get; private set; }
    public string Email { get; private set; } = string.Empty;
    public string FullName { get; private set; } = string.Empty;
    public bool IsActive { get; private set; } = true;
    public DateTime? LastLoginAt { get; private set; }

    private User()
    {
    }

    private User(Guid tenantId, string email, string fullName)
    {
        TenantId = tenantId;
        Email = email;
        FullName = fullName;
    }

    internal static User Create(Guid tenantId, string email, string fullName)
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

        return new User(tenantId, email, fullName);
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
}