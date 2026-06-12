using SharedKernel.Domain.Entities;

namespace Tenant.Identity.Api.Domain.Entities;

public sealed class MunicipalityTenant : AggregateRoot
{
    private readonly List<User> _users = [];

    public string Name { get; private set; } = string.Empty;
    public bool IsActive { get; private set; } = true;
    public IReadOnlyCollection<User> Users => _users.AsReadOnly();

    private MunicipalityTenant()
    {
    }

    private MunicipalityTenant(string name)
    {
        Name = name;
        RegisterCreated();
    }

    private MunicipalityTenant(Guid id, string name)
    {
        Id = id;
        Name = name;
        RegisterCreated();
    }

    public static MunicipalityTenant Create(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Tenant adı zorunludur.", nameof(name));
        }

        return new MunicipalityTenant(name);
    }

    public static MunicipalityTenant Create(Guid id, string name)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("TenantId zorunludur.", nameof(id));
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Tenant adı zorunludur.", nameof(name));
        }

        return new MunicipalityTenant(id, name);
    }

    public User AddUser(string email, string fullName, string passwordHash, string role)
    {
        if (!IsActive)
        {
            throw new InvalidOperationException("Pasif tenant üzerine kullanıcı eklenemez.");
        }

        if (_users.Any(user => string.Equals(user.Email, email, StringComparison.OrdinalIgnoreCase)))
        {
            throw new InvalidOperationException("Bu e-posta ile kullanıcı zaten mevcut.");
        }

        var user = User.Create(Id, email, fullName, passwordHash, role);
        _users.Add(user);
        RegisterUpdated();

        return user;
    }

    public void Deactivate()
    {
        if (!IsActive)
        {
            return;
        }

        IsActive = false;
        RegisterUpdated();
    }
}